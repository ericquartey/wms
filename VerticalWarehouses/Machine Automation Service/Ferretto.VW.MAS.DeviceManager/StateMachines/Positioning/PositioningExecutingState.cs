using System;
using System.Diagnostics;
using System.Threading;

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private const int FindZeroLimit = 80;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly double firstPosition;

        private readonly bool isSimulation;

        private readonly IPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly double secondPosition;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IPositioningStateData stateData;

        private readonly double[] zeroPlateMeasure = new double[4];

        private HorizontalCalibrationStep bayChainFindZeroStep;

        private double bayChainStartingPosition;

        private bool beltBurnishingMovingToInitialPosition;

        private bool beltBurnishingMovingUpwards;

        private int countProfileCalibrated;

        private Timer delayTimer;

        private HorizontalCalibrationStep findZeroStep;

        private double horizontalStartingPosition;

        private bool isDisposed;

        private bool isTestStopped;

        private int performedCycles;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private double? profileCalibratePosition = null;

        private double? profileStartPosition = null;

        private double targetPosition;

        private AxisBounds verticalBounds;

        private double verticalStartingPosition;

        #endregion

        #region Constructors

        public PositioningExecutingState(IPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;

            if (this.machineData?.MessageData.MovementMode == MovementMode.Position
                &&
                this.machineData?.MessageData.MovementType == MovementType.TableTarget)
            {
                if (!this.machineData.MessageData.IsStartedOnBoard)
                {
                    // pickup
                    this.firstPosition = this.machineData.MessageData.SwitchPosition[1]
                                        + (this.machineData.MessageData.SwitchPosition[2] - this.machineData.MessageData.SwitchPosition[1]) / 2;
                    this.secondPosition = this.machineData.MessageData.SwitchPosition[2]
                                        + (this.machineData.MessageData.SwitchPosition[2] - this.machineData.MessageData.SwitchPosition[1]) / 3;
                }
                else
                {
                    // deposit
                    this.firstPosition = this.machineData.MessageData.SwitchPosition[0]
                                        + (this.machineData.MessageData.SwitchPosition[1] - this.machineData.MessageData.SwitchPosition[0]) * 0.75;
                    this.secondPosition = this.machineData.MessageData.SwitchPosition[1]
                                        + (this.machineData.MessageData.SwitchPosition[2] - this.machineData.MessageData.SwitchPosition[1]) / 3;
                }
            }

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            var machine = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>().GetMinMaxHeight();
            this.isSimulation = machine.Simulation;
        }

        #endregion

        #region Properties

        public bool IsBayZeroReached { get; private set; }

        public bool IsStartBayNotZero { get; private set; }

        public bool IsStartPartiallyOnBoard { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.StopTest:
                    if (this.machineData.MessageData.MovementMode == MovementMode.BayTest)
                    {
                        this.Logger.LogInformation($"Stop Bay Test on {this.machineData.RequestingBay} after {this.machineData.MessageData.ExecutedCycles} cycles");
                        this.isTestStopped = true;
                    }

                    if (this.machineData.MessageData.MovementMode == MovementMode.DoubleExtBayTest)
                    {
                        this.Logger.LogInformation($"Stop BED Test on {this.machineData.RequestingBay} after {this.machineData.MessageData.ExecutedCycles} cycles");
                        this.isTestStopped = true;
                    }
                    break;

                default:
                    break;
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationExecuting:
                    switch (message.Type)
                    {
                        case FieldMessageType.InverterStatusUpdate:
                            this.OnInverterStatusUpdated(message);
                            break;
                    }
                    break;

                case MessageStatus.OperationEnd:
                    switch (message.Type)
                    {
                        case FieldMessageType.Positioning:
                            this.Logger.LogDebug($"Trace Notification Message {message.ToString()} Axis:{this.machineData.MessageData.AxisMovement}");
                            this.ProcessEndPositioning();
                            break;

                        case FieldMessageType.InverterStop:
                            this.ProcessEndStop();
                            break;
                    }
                    break;

                case MessageStatus.OperationError:
                    this.stateData.FieldMessage = message;
                    // stop timers
                    this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex} Axis:{this.machineData.MessageData.AxisMovement}");
            FieldCommandMessage commandMessage = null;
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;
            var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                case MovementMode.PositionAndMeasureWeight:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.BayChain:
                case MovementMode.BayChainManual:
                case MovementMode.DoubleExtBayTest:
                case MovementMode.BayTest:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);

                        if (this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                        {
                            this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;
                            this.IsStartPartiallyOnBoard = !(this.machineData.MachineSensorStatus.IsSensorZeroOnCradle
                                || this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle);
                        }
                        else if (this.machineData.MessageData.AxisMovement == Axis.Vertical)
                        {
                            this.verticalStartingPosition = this.elevatorProvider.VerticalPosition;
                            this.verticalBounds = this.elevatorProvider.GetVerticalBounds();
                        }
                        if (this.machineData.MessageData.MovementMode == MovementMode.BayChainManual
                            || this.machineData.MessageData.MovementMode == MovementMode.BayChain)
                        {
                            this.IsStartBayNotZero = !(this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay));
                            this.IsStartPartiallyOnBoard = this.machineData.MessageData.LoadingUnitId.HasValue && this.machineData.MessageData.LoadingUnitId.Value > 0;
                        }

                        if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasureProfile)
                        {
                            var ioCommandMessageData = new MeasureProfileFieldMessageData(true);
                            var ioCommandMessage = new FieldCommandMessage(
                                ioCommandMessageData,
                                $"Measure Profile Start ",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.MeasureProfile,
                                (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

                            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
                        }
                        else if (this.machineData.MessageData.MovementMode == MovementMode.BayTest)
                        {
                            var procedure = this.setupProceduresDataProvider.GetBayCarouselCalibration(this.machineData.RequestingBay);
                            this.performedCycles = procedure.PerformedCycles;
                            this.Logger.LogInformation($"Start BayTest {this.performedCycles} cycle to {this.machineData.MessageData.RequiredCycles}");
                        }
                        else if (this.machineData.MessageData.MovementMode == MovementMode.DoubleExtBayTest)
                        {
                            var procedure = this.setupProceduresDataProvider.GetBayExternalCalibration(this.machineData.RequestingBay);
                            this.performedCycles = procedure.PerformedCycles;
                            this.Logger.LogInformation($"Start BayTest {this.performedCycles} cycle to {this.machineData.MessageData.RequiredCycles}");
                        }
                    }
                    break;

                case MovementMode.TorqueCurrentSampling:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                        statusWordPollingInterval = 500;
                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"Start torque current sampling",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
                    }
                    break;

                case MovementMode.ProfileCalibration:
                    {
                        this.profileCalibratePosition = null;
                        this.profileStartPosition = null;
                        this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;

                        var axis = elevatorDataProvider.GetAxis(Orientation.Horizontal);
                        this.targetPosition = axis.ProfileCalibrateLength + this.horizontalStartingPosition;

                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                        statusWordPollingInterval = 500;
                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"Start profile calibration",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);

                        var ioCommandMessageData = new MeasureProfileFieldMessageData(true);
                        var ioCommandMessage = new FieldCommandMessage(
                            ioCommandMessageData,
                            $"Measure Profile Start ",
                            FieldMessageActor.IoDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.MeasureProfile,
                            (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

                        this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                        this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    {
                        // upwards movement message
                        this.positioningUpFieldMessageData = new PositioningFieldMessageData(
                            new PositioningMessageData(this.machineData.MessageData)
                            {
                                TargetPosition = this.machineData.MessageData.UpperBound
                            }, this.machineData.RequestingBay);

                        // downwards movement message
                        this.positioningDownFieldMessageData = new PositioningFieldMessageData(
                            new PositioningMessageData(this.machineData.MessageData)
                            {
                                TargetPosition = this.machineData.MessageData.LowerBound
                            }, this.machineData.RequestingBay);

                        var procedure = this.setupProceduresDataProvider.GetBeltBurnishingTest();
                        this.performedCycles = procedure.PerformedCycles;

                        // start by moving to lower position
                        this.beltBurnishingMovingToInitialPosition = true;
                        commandMessage = new FieldCommandMessage(
                            this.positioningDownFieldMessageData,
                            "Belt Burninshing Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
                    }
                    break;

                case MovementMode.HorizontalCalibration:
                    {
                        this.findZeroStep = HorizontalCalibrationStep.LeaveZeroSensor;
                        this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;
                        this.targetPosition = Math.Abs(this.machineData.MessageData.TargetPosition) * 0.99;
                        statusWordPollingInterval = 500;

                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning Find Zero Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
                    }
                    break;

                case MovementMode.FindZero:
                    {
                        this.findZeroStep = HorizontalCalibrationStep.FindCenter;
                        this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;
                        this.targetPosition = this.machineData.MessageData.TargetPosition;
                        statusWordPollingInterval = 500;

                        this.Logger.LogDebug($"Start Find zero, TargetPosition={this.targetPosition:0.0000} StartPosition={this.horizontalStartingPosition:0.0000}");

                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning Find lost Zero Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
                    }
                    break;

                case MovementMode.BayChainFindZero:
                    {
                        this.bayChainFindZeroStep = HorizontalCalibrationStep.FindCenter;
                        this.bayChainStartingPosition = this.baysDataProvider.GetChainPosition(this.machineData.RequestingBay);
                        this.targetPosition = this.machineData.MessageData.TargetPosition;
                        statusWordPollingInterval = 500;

                        this.Logger.LogDebug($"Start Find zero, TargetPosition={this.targetPosition:0.0000} StartPosition={this.bayChainStartingPosition:0.0000}");

                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning Find lost Zero Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
                    }
                    break;

                default:
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    break;
            }

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, statusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex));
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method: Start. Reason {reason} Axis:{this.machineData.MessageData.AxisMovement}");

            // stop timers
            this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;
            this.machineData.ExecutedSteps = this.performedCycles;
            if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasureProfile
                || this.machineData.MessageData.MovementMode == MovementMode.ProfileCalibration
                )
            {
                var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Measure Profile Stop ",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.MeasureProfile,
                    (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

                this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
            }

            if (reason == StopRequestReason.Error)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        private void DelayBayChainEnd(object state)
        {
            if (this.IsBracketSensorError())
            {
                this.Logger.LogError($"Bracket sensor error");
                this.errorsProvider.RecordNew(MachineErrorCode.SensorZeroBayNotActiveAtEnd, this.machineData.RequestingBay);
                this.Stop(StopRequestReason.Stop);
            }
            else
            {
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
            }
        }

        private void DelayDoubleExtBayElapsed(object state)
        {
            this.Logger.LogInformation($"Start another BayTest after {this.performedCycles} cycles to {this.machineData.MessageData.RequiredCycles}");

            this.machineData.MessageData.Direction = this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;

            var bay = this.baysDataProvider.GetByNumberExternal(this.machineData.TargetBay);

            var race = bay.External.Race;
            var targetPosition = race;
            switch (this.machineData.MessageData.Direction)
            {
                case HorizontalMovementDirection.Forwards:
                    targetPosition = race - Math.Abs(this.baysDataProvider.GetChainPosition(this.machineData.TargetBay)) - bay.ChainOffset;
                    break;

                case HorizontalMovementDirection.Backwards:
                    targetPosition = bay.ChainOffset - Math.Abs(this.baysDataProvider.GetChainPosition(this.machineData.TargetBay));
                    break;
            }

            this.machineData.MessageData.TargetPosition = targetPosition;

            var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.Positioning,
                inverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, DefaultStatusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex));

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"BayTest {this.machineData.ExecutedSteps} / {this.machineData.MessageData.RequiredCycles}",
                MessageActor.AutomationService,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        private void DelayElapsed(object state)
        {
            this.verticalStartingPosition = this.elevatorProvider.VerticalPosition;

            // INFO Even to go Up and Odd for Down
            var commandMessage = new FieldCommandMessage(
                this.beltBurnishingMovingUpwards
                    ? this.positioningDownFieldMessageData
                    : this.positioningUpFieldMessageData,
                $"Belt Burninshing moving cycle N° {this.performedCycles}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.Positioning,
                (byte)InverterIndex.MainInverter);

            this.beltBurnishingMovingToInitialPosition = false;
            this.beltBurnishingMovingUpwards = !this.beltBurnishingMovingUpwards;

            this.Logger.LogTrace(
                $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var beltBurnishingPosition = this.beltBurnishingMovingUpwards
                ? BeltBurnishingPosition.UpperBound
                : BeltBurnishingPosition.LowerBound;

            this.machineData.MessageData.BeltBurnishingPosition = beltBurnishingPosition;
            this.machineData.MessageData.TargetPosition = this.beltBurnishingMovingUpwards
                ? this.positioningUpFieldMessageData.TargetPosition
                : this.positioningDownFieldMessageData.TargetPosition;

            this.Logger.LogTrace(
                $"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement};");

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"Current position {beltBurnishingPosition}",
                MessageActor.AutomationService,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, DefaultStatusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter));

            if (!this.beltBurnishingMovingUpwards && !this.beltBurnishingMovingToInitialPosition)
            {
                var procedure = this.setupProceduresDataProvider.GetBeltBurnishingTest();
                this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
            }
        }

        private void DelayExtBayElapsed(object state)
        {
            this.Logger.LogInformation($"Start another BayTest after {this.performedCycles} cycles to {this.machineData.MessageData.RequiredCycles}");

            var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.Positioning,
                inverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, DefaultStatusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex));

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"BayTest {this.machineData.ExecutedSteps} / {this.machineData.MessageData.RequiredCycles}",
                MessageActor.AutomationService,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        private void FindZeroNextPosition(double targetPosition)
        {
            this.machineData.MessageData.TargetPosition = targetPosition;
            var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

            var inverterMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                "Continue Message Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ContinueMovement,
                (byte)this.machineData.CurrentInverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            this.Logger.LogDebug($"Continue Message send to inverter {this.machineData.CurrentInverterIndex}, target {targetPosition:0.0000}");
        }

        private bool IsBracketSensorError()
        {
            return !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
        }

        private bool IsHorizontalSensorsError()
        {
            if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle
                && (this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradle
                    || this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle
                    )
                )
            {
                //return true;
                this.Logger.LogWarning("Sensors error during horizontal positioning");
                return false;
            }
            return false;
        }

        private bool IsLoadingErrorDuringPickup()
        {
            if (!this.machineData.MessageData.IsStartedOnBoard
                && !this.isSimulation)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.elevatorProvider.HorizontalPosition > this.firstPosition
                        && this.elevatorProvider.HorizontalPosition < this.secondPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradle
                        )
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.elevatorProvider.HorizontalPosition < this.firstPosition
                        && this.elevatorProvider.HorizontalPosition > this.secondPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradle
                        )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsSensorsError(Axis axisMovement)
        {
            //if (axisMovement == Axis.Horizontal)
            //{
            //    return this.IsHorizontalSensorsError();
            //}
            if (axisMovement == Axis.Vertical)
            {
                return this.IsVerticalSensorsError();
            }
            return false;
        }

        private bool IsUnloadingErrorDuringDeposit()
        {
            if (this.machineData.MessageData.IsStartedOnBoard
                && !this.isSimulation)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.elevatorProvider.HorizontalPosition > this.firstPosition
                        && this.elevatorProvider.HorizontalPosition < this.secondPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradle
                        )
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.elevatorProvider.HorizontalPosition < this.firstPosition
                        && this.elevatorProvider.HorizontalPosition > this.secondPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradle
                        )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsVerticalSensorsError()
        {
            if (this.machineData.MessageData.IsStartedOnBoard
                && !(this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                )
            {
                return true;
            }
            if (!this.machineData.MessageData.IsStartedOnBoard
                && !(this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                )
            {
                return true;
            }

            return false;
        }

        private bool IsVerticalZeroHighError()
        {
            return this.machineData.MessageData.MovementMode == MovementMode.Position
                && this.machineData.MessageData.AxisMovement == Axis.Vertical
                && !this.machineData.MessageData.BypassConditions
                && this.machineData.MachineSensorStatus.IsSensorZeroOnElevator
                && this.elevatorProvider.VerticalPosition > this.verticalBounds.Offset * 1.4;
        }

        private bool IsVerticalZeroLowError()
        {
            return this.machineData.MessageData.MovementMode == MovementMode.Position
                && this.machineData.MessageData.AxisMovement == Axis.Vertical
                && !this.machineData.MessageData.BypassConditions
                && !this.machineData.MachineSensorStatus.IsSensorZeroOnElevator
                && !this.isSimulation
                && this.elevatorProvider.VerticalPosition < this.verticalBounds.Offset * 0.7;
        }

        private bool IsZeroSensorError()
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.Position
                && this.machineData.MessageData.MovementType == MovementType.TableTarget
                && ((this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    || (this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    )
                )
            {
                return true;
            }

            return false;
        }

        private void OnInverterStatusUpdated(FieldNotificationMessage message)
        {
            Debug.Assert(message.Data is InverterStatusUpdateFieldMessageData);

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.BayChainManual:
                    {
                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                        {
                            if (this.IsStartBayNotZero)
                            {
                                this.IsBayZeroReached = true;
                            }

                            if (this.IsStartPartiallyOnBoard
                                && this.IsStartBayNotZero
                                && this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.RequestingBay))
                            {
                                var data = new PositioningMessageData();
                                data.MovementType = this.machineData.MessageData.MovementType;
                                data.MovementMode = this.machineData.MessageData.MovementMode;
                                data.AxisMovement = this.machineData.MessageData.AxisMovement;

                                this.Logger.LogDebug($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement}; Zero Sensor {this.machineData.MachineSensorStatus.IsSensorZeroOnCradle}");
                                var notificationMessage = new NotificationMessage(
                                    data,
                                    $"Manual movement aborted",
                                    MessageActor.MachineManager,
                                    MessageActor.DeviceManager,
                                    MessageType.Positioning,
                                    this.machineData.RequestingBay,
                                    this.machineData.TargetBay,
                                    MessageStatus.OperationUpdateData);

                                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                                // do not repeat notification
                                this.IsStartPartiallyOnBoard = false;
                            }
                        }
                        else if (!this.IsStartBayNotZero)
                        {
                            this.IsStartBayNotZero = true;
                        }
                        if (this.IsBayZeroReached && !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                        {
                            this.Logger.LogWarning("Bay chain in wrong position!");
                            this.errorsProvider.RecordNew(MachineErrorCode.SensorZeroBayNotActiveAtEnd, this.machineData.RequestingBay);

                            //this.stateData.FieldMessage = message;
                            this.Stop(StopRequestReason.Stop);
                            this.IsBayZeroReached = false;
                        }
                    }
                    break;

                case MovementMode.BayChain:
                    {
                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                        {
                            if (this.IsStartBayNotZero)
                            {
                                this.IsBayZeroReached = true;
                            }
                        }
                        else if (!this.IsStartBayNotZero)
                        {
                            this.IsStartBayNotZero = true;
                        }
                        if (this.IsBayZeroReached && !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                        {
                            this.Logger.LogWarning("Bay chain in wrong position!");
                            this.errorsProvider.RecordNew(MachineErrorCode.SensorZeroBayNotActiveAtEnd, this.machineData.RequestingBay);

                            //this.stateData.FieldMessage = message;
                            this.Stop(StopRequestReason.Stop);
                            this.IsBayZeroReached = false;
                        }
                    }
                    break;

                case MovementMode.HorizontalCalibration:
                    {
                        if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        {
                            var data = message.Data as InverterStatusUpdateFieldMessageData;
                            var chainPosition = data.CurrentPosition;

                            if (chainPosition.HasValue)
                            {
                                switch (this.findZeroStep)
                                {
                                    case HorizontalCalibrationStep.LeaveZeroSensor:
                                        if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                                        {
                                            this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");

                                            if (this.zeroPlateMeasure[0] != 0)
                                            {
                                                this.findZeroStep = HorizontalCalibrationStep.BackwardFindZeroSensor;
                                                this.zeroPlateMeasure[2] = chainPosition.Value;
                                            }
                                            else
                                            {
                                                this.findZeroStep = HorizontalCalibrationStep.ForwardFindZeroSensor;
                                                this.zeroPlateMeasure[0] = chainPosition.Value;
                                            }

                                            var invertDirection = (this.machineData.MessageData.TargetPosition > 0) ? -1 : 1;
                                            this.FindZeroNextPosition(invertDirection * Math.Abs(this.machineData.MessageData.TargetPosition));
                                        }
                                        break;

                                    case HorizontalCalibrationStep.ForwardFindZeroSensor:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                                        {
                                            if (this.zeroPlateMeasure[1] == 0)
                                            {
                                                this.zeroPlateMeasure[1] = chainPosition.Value;
                                            }

                                            this.findZeroStep = HorizontalCalibrationStep.LeaveZeroSensor;
                                            this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                                        }
                                        break;

                                    case HorizontalCalibrationStep.BackwardFindZeroSensor:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                                        {
                                            if (this.zeroPlateMeasure[3] == 0)
                                            {
                                                this.zeroPlateMeasure[3] = chainPosition.Value;
                                            }

                                            this.findZeroStep = HorizontalCalibrationStep.FindCenter;
                                            this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                                        }
                                        break;

                                    case HorizontalCalibrationStep.FindCenter:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle && this.machineData.MessageData.TargetPosition != 0.0)
                                        {
                                            this.FindZeroNextPosition(0.0);
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }

                case MovementMode.FindZero:
                    {
                        if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        {
                            var data = message.Data as InverterStatusUpdateFieldMessageData;
                            var chainPosition = data.CurrentPosition;

                            if (chainPosition.HasValue)
                            {
                                switch (this.findZeroStep)
                                {
                                    case HorizontalCalibrationStep.FindCenter:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                                        {
                                            this.Stop(StopRequestReason.Stop);
                                            this.Logger.LogDebug($"Horizontal Find Zero operation Stop, Value {chainPosition:0.0000}");
                                        }
                                        else if ((data.CurrentPosition.Value + 1 >= this.horizontalStartingPosition + FindZeroLimit + 1) &&
                                            (data.CurrentPosition.Value - 1 <= this.horizontalStartingPosition + FindZeroLimit + 1))
                                        {
                                            this.Logger.LogDebug($"Horizontal Find Zero update destination position Value {this.horizontalStartingPosition - (FindZeroLimit * 2):0.0000}");

                                            this.findZeroStep = HorizontalCalibrationStep.BackwardFindZeroSensor;
                                            this.FindZeroNextPosition(this.horizontalStartingPosition - (FindZeroLimit * 2));
                                        }
                                        break;

                                    case HorizontalCalibrationStep.BackwardFindZeroSensor:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                                        {
                                            this.Stop(StopRequestReason.Stop);
                                            this.Logger.LogDebug($"Horizontal Find Zero operation Stop, Value {chainPosition:0.0000}");
                                        }
                                        else if ((data.CurrentPosition.Value + 1 >= this.horizontalStartingPosition - FindZeroLimit - 1) &&
                                            (data.CurrentPosition.Value - 1 <= this.horizontalStartingPosition - FindZeroLimit - 1))
                                        {
                                            this.Logger.LogDebug($"Horizontal Find Zero update destination position Value {this.horizontalStartingPosition:0.0000}");

                                            this.findZeroStep = HorizontalCalibrationStep.ForwardFindZeroSensor;
                                            this.FindZeroNextPosition(this.horizontalStartingPosition);
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }

                case MovementMode.BayChainFindZero:
                    {
                        if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        {
                            var data = message.Data as InverterStatusUpdateFieldMessageData;
                            var chainPosition = data.CurrentPosition;

                            if (chainPosition.HasValue)
                            {
                                var bayFindZeroLimit = this.baysDataProvider.GetCarouselBayFindZeroLimit(this.machineData.RequestingBay);
                                bayFindZeroLimit = bayFindZeroLimit == 0 ? 6 : bayFindZeroLimit;
                                switch (this.bayChainFindZeroStep)
                                {
                                    case HorizontalCalibrationStep.FindCenter:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                                        {
                                            this.Stop(StopRequestReason.Stop);
                                            this.Logger.LogDebug($"BayChain Find Zero operation Stop, Value {chainPosition:0.0000}");
                                        }
                                        else if ((data.CurrentPosition.Value + 1 >= this.bayChainStartingPosition - bayFindZeroLimit + 1) &&
                                            (data.CurrentPosition.Value - 1 <= this.bayChainStartingPosition - bayFindZeroLimit + 1))
                                        {
                                            this.Logger.LogDebug($"BayChain Find Zero update destination position Value {this.bayChainStartingPosition + (bayFindZeroLimit * 2):0.0000}");

                                            this.bayChainFindZeroStep = HorizontalCalibrationStep.BackwardFindZeroSensor;
                                            this.FindZeroNextPosition(this.bayChainStartingPosition + (bayFindZeroLimit * 2));
                                        }
                                        break;

                                    case HorizontalCalibrationStep.BackwardFindZeroSensor:
                                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                                        {
                                            this.Stop(StopRequestReason.Stop);
                                            this.Logger.LogDebug($"BayChain Find Zero operation Stop, Value {chainPosition:0.0000}");
                                        }
                                        else if ((data.CurrentPosition.Value + 1 >= this.bayChainStartingPosition + bayFindZeroLimit - 1) &&
                                            (data.CurrentPosition.Value - 1 <= this.bayChainStartingPosition + bayFindZeroLimit - 1))
                                        {
                                            this.Logger.LogDebug($"BayChain Find Zero update destination position Value {this.bayChainStartingPosition:0.0000}");

                                            this.bayChainFindZeroStep = HorizontalCalibrationStep.ForwardFindZeroSensor;
                                            this.FindZeroNextPosition(this.bayChainStartingPosition);
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }

                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.TableTarget:
                    {
                        if (this.IsLoadingErrorDuringPickup())
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);

                            this.Stop(StopRequestReason.Stop);
                        }
                        else if (this.IsUnloadingErrorDuringDeposit())
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }

                        //if (this.IsHorizontalSensorsError())
                        //{
                        //    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                        //    this.stateData.FieldMessage = message;
                        //    this.Stop(StopRequestReason.Error);
                        //}
                        break;
                    }

                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.Absolute:
                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.Relative:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.PositionAndMeasureWeight:
                    {
                        if (!this.machineData.MessageData.BypassConditions
                            && this.IsSensorsError(this.machineData.MessageData.AxisMovement)
                            )
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                            this.stateData.FieldMessage = message;
                            this.Stop(StopRequestReason.Error);
                        }
                        else if (this.machineData.MessageData.MovementMode == MovementMode.Position
                            && this.machineData.MessageData.AxisMovement == Axis.Horizontal
                            && this.IsStartPartiallyOnBoard
                            && (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle
                                || this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                            )
                        {
                            var data = new PositioningMessageData();
                            data.MovementType = this.machineData.MessageData.MovementType;
                            data.AxisMovement = this.machineData.MessageData.AxisMovement;

                            this.Logger.LogDebug($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement}; Zero Sensor {this.machineData.MachineSensorStatus.IsSensorZeroOnCradle}");
                            var notificationMessage = new NotificationMessage(
                                data,
                                $"Manual movement aborted",
                                MessageActor.MachineManager,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                MessageStatus.OperationUpdateData);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                            // do not repeat notification
                            this.IsStartPartiallyOnBoard = false;
                        }
                        break;
                    }

                case MovementMode.ProfileCalibration:
                    {
                        if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        {
                            var data = message.Data as InverterStatusUpdateFieldMessageData;
                            var chainPosition = data.CurrentPosition;

                            var machineResourcesProvider = this.scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
                            if (machineResourcesProvider.IsProfileCalibratedBay(this.machineData.RequestingBay))
                            {
                                if (this.countProfileCalibrated == 0
                                    && !this.profileStartPosition.HasValue)
                                {
                                    this.profileStartPosition = chainPosition;
                                    this.Logger.LogInformation($"profileStartPosition = {this.profileStartPosition.Value:0.0000}");
                                }
                                else if (this.countProfileCalibrated == 1
                                    && !this.profileCalibratePosition.HasValue)
                                {
                                    if (chainPosition.HasValue && this.profileStartPosition.HasValue)
                                    {
                                        this.profileCalibratePosition = data.CurrentPosition;
                                    }

                                    this.Logger.LogInformation($"profileCalibratePosition reached! Value {this.profileCalibratePosition.Value:0.0000}");
                                    this.ReturnToStartPosition();
                                    this.countProfileCalibrated = 2;
                                }
                            }
                            else if (this.countProfileCalibrated == 0
                                && this.profileStartPosition.HasValue)
                            {
                                // profileCalibrated signal is low after startPosition
                                this.countProfileCalibrated = 1;
                            }
                            else if (chainPosition.HasValue
                                && this.countProfileCalibrated <= 2
                                && Math.Abs(chainPosition.Value - this.horizontalStartingPosition) >= this.targetPosition
                                )
                            {
                                if (this.countProfileCalibrated < 2)
                                {
                                    this.Logger.LogInformation($"profileCalibratePosition NOT reached!");
                                }
                                this.countProfileCalibrated = 2;
                                this.ReturnToStartPosition();
                            }
                        }
                        break;
                    }

                case MovementMode.BeltBurnishing:
                    if (this.IsSensorsError(this.machineData.MessageData.AxisMovement))
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                        this.stateData.FieldMessage = message;
                        this.Stop(StopRequestReason.Error);
                    }
                    break;
            }

            if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
            {
                var data = message.Data as InverterStatusUpdateFieldMessageData;

                this.machineData.MessageData.TorqueCurrentSample = data.TorqueCurrent;

                this.Logger.LogTrace($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement};");
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"Current Encoder position changed",
                    MessageActor.AutomationService,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationExecuting);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        private void ProcessEndPositioning()
        {
            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.PositionAndMeasureWeight:
                case MovementMode.ProfileCalibration:
                    {
                        this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode, Movement axis: {this.machineData.MessageData.AxisMovement}");
                        this.machineData.ExecutedSteps = this.performedCycles;

                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = 0.0;
                        if (this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                        {
                            distance = Math.Abs(this.elevatorProvider.HorizontalPosition - this.horizontalStartingPosition);
                            if (distance > 200)
                            {
                                machineProvider.UpdateHorizontalAxisStatistics(distance);
                            }
                        }
                        else if (this.machineData.MessageData.AxisMovement == Axis.Vertical)
                        {
                            distance = Math.Abs(this.elevatorProvider.VerticalPosition - this.verticalStartingPosition);
                            if (distance > 100)
                            {
                                machineProvider.UpdateVerticalAxisStatistics(distance);
                            }
                        }

                        if (!this.machineData.MessageData.BypassConditions
                            && this.IsZeroSensorError()
                            )
                        {
                            if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                            {
                                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.ZeroSensorErrorAfterPickup, this.machineData.RequestingBay);
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.ZeroSensorErrorAfterDeposit, this.machineData.RequestingBay);
                            }
                            this.Stop(StopRequestReason.Stop);
                        }
                        else if (this.machineData.MessageData.MovementType == MovementType.TableTarget
                            && !this.machineData.MessageData.IsStartedOnBoard
                            && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }
                        else if (this.machineData.MessageData.MovementType == MovementType.TableTarget
                            && this.machineData.MessageData.IsStartedOnBoard
                            && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }
                        else
                        {
                            if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasureProfile
                                || this.machineData.MessageData.MovementMode == MovementMode.ProfileCalibration
                                )
                            {
                                var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
                                var ioCommandMessage = new FieldCommandMessage(
                                    ioCommandMessageData,
                                    $"Measure Profile Stop ",
                                    FieldMessageActor.IoDriver,
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageType.MeasureProfile,
                                    (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

                                this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                                if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasureProfile)
                                {
                                    this.ParentStateMachine.ChangeState(new PositioningProfileState(this.stateData, this.Logger));
                                }
                                else
                                {
                                    double profileCalibrateDistance = 0;
                                    double profileStartDistance = 0;
                                    if (this.profileStartPosition.HasValue)
                                    {
                                        profileStartDistance = Math.Abs(this.profileStartPosition.Value - this.horizontalStartingPosition);
                                    }
                                    if (this.profileCalibratePosition.HasValue && this.profileStartPosition.HasValue)
                                    {
                                        profileCalibrateDistance = Math.Abs(this.profileCalibratePosition.Value - this.profileStartPosition.Value);
                                    }

                                    var procedure = this.setupProceduresDataProvider.GetBayProfileCheck(this.machineData.RequestingBay);

                                    double measured = 0;

                                    var radians = procedure.ProfileDegrees * (Math.PI / 180);
                                    if (profileStartDistance != 0 && profileCalibrateDistance != 0)
                                    {
                                        measured = (double)((procedure.ProfileCorrectDistance - profileCalibrateDistance) * Math.Tan(radians));
                                    }
                                    else
                                    {
                                        measured = (-procedure.ProfileTotalDistance) * Math.Tan(radians) / 2;
                                    }
                                    this.Logger.LogDebug($"Send Profile calibration result: Calibrate Distance {profileCalibrateDistance:0.0000}, Start Distance {profileStartDistance:0.0000}, measured {measured:0.0000}");

                                    var notificationMessage = new NotificationMessage(
                                        new ProfileCalibrationMessageData(profileStartDistance, profileCalibrateDistance, measured),
                                        $"Profile calibration result",
                                        MessageActor.AutomationService,
                                        MessageActor.DeviceManager,
                                        MessageType.ProfileCalibration,
                                        this.machineData.RequestingBay,
                                        this.machineData.TargetBay,
                                        MessageStatus.OperationEnd);

                                    this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                                }
                            }
                            else
                            {
                                // stop timers
                                this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                            }
                        }
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    {
                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test &&
                            machineModeProvider.Mode != MachineMode.Test2 &&
                            machineModeProvider.Mode != MachineMode.Test3)
                        {
                            switch (this.machineData.TargetBay)
                            {
                                case BayNumber.BayOne:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;

                                case BayNumber.BayTwo:
                                    machineModeProvider.Mode = MachineMode.Test2;
                                    break;

                                case BayNumber.BayThree:
                                    machineModeProvider.Mode = MachineMode.Test3;
                                    break;

                                default:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;
                            }

                            this.Logger.LogInformation($"Machine status switched to {machineModeProvider.Mode}");
                        }
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.elevatorProvider.VerticalPosition - this.verticalStartingPosition);
                        if (distance > 100)
                        {
                            machineProvider.UpdateVerticalAxisStatistics(distance);
                        }

                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        if (this.performedCycles >= this.machineData.MessageData.RequiredCycles)
                        {
                            this.Logger.LogDebug("FSM Finished Executing State");
                            this.machineData.ExecutedSteps = this.performedCycles;
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                        }
                        else
                        {
                            if (this.machineData.MessageData.DelayEnd > 0)
                            {
                                this.delayTimer = new Timer(this.DelayElapsed, null, this.machineData.MessageData.DelayEnd * 1000, Timeout.Infinite);
                            }
                            else
                            {
                                this.DelayElapsed(null);
                            }
                        }
                    }
                    break;

                case MovementMode.HorizontalCalibration:
                    {
                        this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode");
                        this.machineData.ExecutedSteps = this.performedCycles;
                        //var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                        var axis = elevatorDataProvider.GetAxis(Orientation.Horizontal);
                        double profileOriginalDistance = (double)(axis.ChainOffset);
                        //double profileCalibrateDistance = 0;
                        double measured = 0;
                        if (this.findZeroStep == HorizontalCalibrationStep.FindCenter && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        {
                            //profileCalibrateDistance = Math.Abs(this.elevatorProvider.HorizontalPosition);

                            //measured = Math.Abs(this.zeroPlateMeasure[0] + this.zeroPlateMeasure[1]) / 2 +
                            //    Math.Abs(this.zeroPlateMeasure[2] + this.zeroPlateMeasure[3]) / 2;
                            //measured /= -2;

                            var sensorLength = Math.Abs(this.zeroPlateMeasure[0] - this.zeroPlateMeasure[1]) +
                                this.zeroPlateMeasure[2] - this.zeroPlateMeasure[3];
                            measured = -((this.zeroPlateMeasure[2] + Math.Abs(this.zeroPlateMeasure[0])) / 2 - sensorLength);
                        }
                        this.Logger.LogDebug($"Send Horizontal calibration result: {this.zeroPlateMeasure[0]:0.00}, {this.zeroPlateMeasure[1]:0.00}, {this.zeroPlateMeasure[2]:0.00}, {this.zeroPlateMeasure[3]:0.00}, measured {measured:0.00}");

                        var notificationMessage = new NotificationMessage(
                            new ProfileCalibrationMessageData(profileOriginalDistance, measured - profileOriginalDistance, measured),
                            $"Horizontal calibration result",
                            MessageActor.AutomationService,
                            MessageActor.DeviceManager,
                            MessageType.ProfileCalibration,
                            this.machineData.RequestingBay,
                            this.machineData.TargetBay,
                            MessageStatus.OperationEnd);

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                    }
                    break;

                case MovementMode.FindZero:
                    {
                        this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode");

                        if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.MissingZeroSensorWithEmptyElevator, this.machineData.RequestingBay);
                        }

                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                    }
                    break;

                case MovementMode.BayChainFindZero:
                    {
                        this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode");

                        if (!this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay))
                        {
                            this.errorsProvider.RecordNew(MachineErrorCode.SensorZeroBayNotActiveAtEnd, this.machineData.RequestingBay);
                        }

                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                    }
                    break;

                case MovementMode.BayChain:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        if (!this.machineData.MessageData.BypassConditions
                            && this.IsBracketSensorError()
                            )
                        {
                            this.Logger.LogWarning($"Bracket sensor error - try again");
                            this.delayTimer = new Timer(this.DelayBayChainEnd, null, 300, Timeout.Infinite);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                        }
                    }
                    break;

                case MovementMode.BayChainManual:
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                    break;

                case MovementMode.BayTest:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test &&
                            machineModeProvider.Mode != MachineMode.Test2 &&
                            machineModeProvider.Mode != MachineMode.Test3)
                        {
                            switch (this.machineData.TargetBay)
                            {
                                case BayNumber.BayOne:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;

                                case BayNumber.BayTwo:
                                    machineModeProvider.Mode = MachineMode.Test2;
                                    break;

                                case BayNumber.BayThree:
                                    machineModeProvider.Mode = MachineMode.Test3;
                                    break;

                                default:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;
                            }

                            this.Logger.LogInformation($"Machine status switched to {machineModeProvider.Mode}");
                        }

                        var procedure = this.setupProceduresDataProvider.GetBayCarouselCalibration(this.machineData.RequestingBay);
                        this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        MessageStatus status;
                        if (this.IsBracketSensorError())
                        {
                            this.Logger.LogError($"Bracket sensor error");
                            this.Stop(StopRequestReason.Error);
                            break;
                        }
                        else
                        {
                            if (this.performedCycles >= this.machineData.MessageData.RequiredCycles
                                || this.isTestStopped
                                )
                            {
                                this.Logger.LogDebug("FSM Finished Executing State");
                                this.machineData.ExecutedSteps = this.performedCycles;
                                this.machineData.MessageData.IsTestStopped = this.isTestStopped;
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                                break;
                            }
                            else
                            {
                                this.Logger.LogInformation($"Start another BayTest after {this.performedCycles} cycles to {this.machineData.MessageData.RequiredCycles}");

                                var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                                var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

                                var commandMessage = new FieldCommandMessage(
                                    positioningFieldMessageData,
                                    $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageType.Positioning,
                                    inverterIndex);
                                this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

                                this.ParentStateMachine.PublishFieldCommandMessage(
                                    new FieldCommandMessage(
                                        new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, DefaultStatusWordPollingInterval),
                                    "Update Inverter status word status",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageType.InverterSetTimer,
                                    inverterIndex));

                                status = MessageStatus.OperationExecuting;
                            }
                        }
                        var notificationMessage = new NotificationMessage(
                            this.machineData.MessageData,
                            $"BayTest {this.machineData.ExecutedSteps} / {this.machineData.MessageData.RequiredCycles}",
                            MessageActor.AutomationService,
                            MessageActor.DeviceManager,
                            MessageType.Positioning,
                            this.machineData.RequestingBay,
                            this.machineData.TargetBay,
                            status);

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                    }
                    break;

                case MovementMode.DoubleExtBayTest:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test &&
                            machineModeProvider.Mode != MachineMode.Test2 &&
                            machineModeProvider.Mode != MachineMode.Test3)
                        {
                            switch (this.machineData.TargetBay)
                            {
                                case BayNumber.BayOne:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;

                                case BayNumber.BayTwo:
                                    machineModeProvider.Mode = MachineMode.Test2;
                                    break;

                                case BayNumber.BayThree:
                                    machineModeProvider.Mode = MachineMode.Test3;
                                    break;

                                default:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;
                            }

                            this.Logger.LogInformation($"Machine status switched to {machineModeProvider.Mode}");
                        }

                        var procedure = this.setupProceduresDataProvider.GetBayExternalCalibration(this.machineData.RequestingBay);
                        this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        if (this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay) &&
                            this.machineData.MachineSensorStatus.IsSensorZeroTopOnBay(this.machineData.TargetBay))
                        {
                            this.Logger.LogError($"Bracket sensor error");
                            this.Stop(StopRequestReason.Error);
                            break;
                        }
                        else
                        {
                            if (this.performedCycles >= this.machineData.MessageData.RequiredCycles
                                || this.isTestStopped
                                )
                            {
                                this.Logger.LogDebug("FSM Finished Executing State");
                                this.machineData.ExecutedSteps = this.performedCycles;
                                this.machineData.MessageData.IsTestStopped = this.isTestStopped;
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                                break;
                            }
                            else
                            {
                                if (this.machineData.MessageData.DelayEnd > 0)
                                {
                                    this.delayTimer = new Timer(this.DelayDoubleExtBayElapsed, null, this.machineData.MessageData.DelayEnd * 1000, Timeout.Infinite);
                                }
                                else
                                {
                                    this.DelayDoubleExtBayElapsed(null);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ||
                this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle ||
                this.machineData.MessageData.MovementMode == MovementMode.BayChain ||
                this.machineData.MessageData.MovementMode == MovementMode.BayChainManual
                )
            {
                this.machineData.ExecutedSteps = this.performedCycles;
                // stop timers
                this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
            }
        }

        private void ReturnToStartPosition()
        {
            var inverterMessage = new FieldCommandMessage(
                null,
                "Continue Message Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ContinueMovement,
                (byte)this.machineData.CurrentInverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            this.Logger.LogDebug($"Continue Message send to inverter {this.machineData.CurrentInverterIndex} Axis:{this.machineData.MessageData.AxisMovement}");
        }

        #endregion
    }
}
