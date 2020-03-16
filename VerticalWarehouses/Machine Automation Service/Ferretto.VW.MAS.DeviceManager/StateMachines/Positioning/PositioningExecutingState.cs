using System;
using System.Diagnostics;
using System.Linq;
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

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly double firstPosition;

        private readonly IPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly double secondPosition;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IPositioningStateData stateData;

        private bool beltBurnishingMovingToInitialPosition;

        private bool beltBurnishingMovingUpwards;

        private int countProfileCalibrated;

        private Timer delayTimer;

        private double? findZeroPosition = null;

        private double horizontalStartingPosition;

        private bool isDisposed;

        private bool isTestStopped;

        private int performedCycles;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private double? profileCalibratePosition = null;

        private double? profileStartPosition = null;

        private double targetPosition;

        private double verticalStartingPosition;

        #endregion

        #region Constructors

        public PositioningExecutingState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;

            if (this.machineData?.MessageData.MovementMode == MovementMode.Position
                &&
                this.machineData?.MessageData.MovementType == MovementType.TableTarget)
            {
                this.firstPosition = this.machineData.MessageData.SwitchPosition[1]
                                    + (this.machineData.MessageData.SwitchPosition[2] - this.machineData.MessageData.SwitchPosition[1]) / 2;
                this.secondPosition = this.machineData.MessageData.SwitchPosition[2]
                                    + (this.machineData.MessageData.SwitchPosition[2] - this.machineData.MessageData.SwitchPosition[1]) / 2;
            }

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
        }

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
                        case FieldMessageType.InverterStatusUpdate when message.Data is InverterStatusUpdateFieldMessageData:
                            this.OnInverterStatusUpdated(message);
                            break;
                    }
                    break;

                case MessageStatus.OperationEnd:
                    switch (message.Type)
                    {
                        case FieldMessageType.Positioning:
                            this.Logger.LogDebug($"Trace Notification Message {message.ToString()}");
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
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex}");
            FieldCommandMessage commandMessage = null;
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                case MovementMode.PositionAndMeasureWeight:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.BayChain:
                case MovementMode.BayChainManual:
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
                        }
                        else if (this.machineData.MessageData.AxisMovement == Axis.Vertical)
                        {
                            this.verticalStartingPosition = this.elevatorProvider.VerticalPosition;
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

                        var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                        var axis = elevatorDataProvider.GetAxis(Orientation.Horizontal);
                        this.targetPosition = axis.ProfileCalibrateLength;

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

                case MovementMode.FindZero:
                    {
                        this.findZeroPosition = null;
                        this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;
                        this.targetPosition = Math.Abs(this.machineData.MessageData.TargetPosition);
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
            this.Logger.LogDebug($"1:Stop Method Start. Reason {reason}");

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
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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

        private void FindZeroNextPosition()
        {
            var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            var axis = elevatorDataProvider.GetAxis(Orientation.Horizontal);
            this.machineData.MessageData.TargetPosition = axis.ChainOffset * 2;
            var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

            var inverterMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                "Continue Message Command",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ContinueMovement,
                (byte)this.machineData.CurrentInverterIndex);
            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            this.Logger.LogDebug($"Continue Message send to inverter {this.machineData.CurrentInverterIndex}");
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
            if (!this.machineData.MessageData.IsStartedOnBoard)
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
            if (this.machineData.MessageData.IsStartedOnBoard)
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
                case MovementMode.FindZero:
                    {
                        if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        {
                            var data = message.Data as InverterStatusUpdateFieldMessageData;
                            var chainPosition = data.CurrentPosition;
                            if (chainPosition.HasValue
                                && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle
                                && Math.Abs(this.horizontalStartingPosition - chainPosition.Value) > 25
                                )
                            {
                                if (this.findZeroPosition is null)
                                {
                                    this.findZeroPosition = chainPosition;
                                    this.Logger.LogInformation($"first Zero sensor reached! Value {chainPosition:0.0000}");
                                    this.FindZeroNextPosition();
                                }
                                else
                                {
                                    // TODO finish procedure
                                }
                            }
                        }

                        break;
                    }

                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.TableTarget:
                    {
                        if (this.IsLoadingErrorDuringPickup())
                        {
                            this.Logger.LogWarning("Cradle not correctly loaded during pickup");
                            //this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);

                            //this.stateData.FieldMessage = message;
                            //this.Stop(StopRequestReason.Stop);
                        }
                        else if (this.IsUnloadingErrorDuringDeposit())
                        {
                            this.Logger.LogWarning("Cradle not correctly unloaded during deposit");
                            //this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
                            //this.stateData.FieldMessage = message;
                            //this.Stop(StopRequestReason.Stop);
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
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                            this.stateData.FieldMessage = message;
                            this.Stop(StopRequestReason.Error);
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
                                && this.countProfileCalibrated < 2
                                && Math.Abs(chainPosition.Value - this.horizontalStartingPosition) >= this.targetPosition
                                )
                            {
                                this.countProfileCalibrated = 2;
                                this.Logger.LogInformation($"profileCalibratePosition NOT reached!");
                                this.ReturnToStartPosition();
                            }
                        }
                        break;
                    }

                case MovementMode.BeltBurnishing:
                    if (this.IsSensorsError(this.machineData.MessageData.AxisMovement))
                    {
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

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
                        this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode");
                        this.machineData.ExecutedSteps = this.performedCycles;

                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        double distance = 0.0;
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
                            if (distance > 50)
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
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }
                        else if (this.machineData.MessageData.MovementType == MovementType.TableTarget
                            && this.machineData.MessageData.IsStartedOnBoard
                            && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle)
                        {
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
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
                                    this.ParentStateMachine.ChangeState(new PositioningProfileState(this.stateData));
                                }
                                else
                                {
                                    double? profileCalibrateDistance = null;
                                    double? profileStartDistance = null;
                                    if (this.profileStartPosition.HasValue)
                                    {
                                        profileStartDistance = Math.Abs(this.profileStartPosition.Value - this.horizontalStartingPosition);
                                    }
                                    if (this.profileCalibratePosition.HasValue && this.profileStartPosition.HasValue)
                                    {
                                        profileCalibrateDistance = Math.Abs(this.profileCalibratePosition.Value - this.profileStartPosition.Value);
                                    }
                                    this.Logger.LogDebug($"Send Profile calibration result: Calibrate Distance {profileCalibrateDistance:0.0000}, Start Distance {profileStartDistance:0.0000}");

                                    var procedure = this.setupProceduresDataProvider.GetBayProfileCheck(this.machineData.RequestingBay);

                                    double? measured = null;

                                    var radians = procedure.ProfileDegrees * (Math.PI / 180);
                                    if (profileStartDistance.HasValue && profileCalibrateDistance.HasValue)
                                    {
                                        measured = (procedure.ProfileCorrectDistance - profileCalibrateDistance) * Math.Tan(radians);
                                    }
                                    else
                                    {
                                        measured = (-procedure.ProfileTotalDistance) * Math.Tan(radians) / 2;
                                    }

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

                                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                                }
                            }
                            else
                            {
                                // stop timers
                                this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                            }
                        }
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    {
                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test)
                        {
                            machineModeProvider.Mode = MachineMode.Test;
                            this.Logger.LogInformation($"Machine status switched to {MachineMode.Test}");
                        }
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        double distance = Math.Abs(this.elevatorProvider.VerticalPosition - this.verticalStartingPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateVerticalAxisStatistics(distance);
                        }

                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        if (this.performedCycles >= this.machineData.MessageData.RequiredCycles)
                        {
                            this.Logger.LogDebug("FSM Finished Executing State");
                            this.machineData.ExecutedSteps = this.performedCycles;
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                        }
                        else
                        {
                            if (this.machineData.MessageData.Delay > 0)
                            {
                                this.delayTimer = new Timer(this.DelayElapsed, null, this.machineData.MessageData.Delay * 1000, Timeout.Infinite);
                            }
                            else
                            {
                                this.DelayElapsed(null);
                            }
                        }
                    }
                    break;

                case MovementMode.FindZero:
                    this.machineData.ExecutedSteps = this.performedCycles;
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    break;

                case MovementMode.BayChain:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        double distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        if (!this.machineData.MessageData.BypassConditions
                            && this.IsBracketSensorError()
                            )
                        {
                            this.Logger.LogError($"Bracket sensor error");
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.SensorZeroBayNotActiveAtEnd, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                        }
                    }
                    break;

                case MovementMode.BayChainManual:
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    break;

                case MovementMode.BayTest:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        double distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test)
                        {
                            machineModeProvider.Mode = MachineMode.Test;
                            this.Logger.LogInformation($"Machine status switched to {MachineMode.Test}");
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
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
            }
            else if (
                this.machineData.MessageData.MovementMode == MovementMode.Position ||       // TODO: could we remove this line???? please??
                this.machineData.MessageData.MovementMode == MovementMode.FindZero
                )
            {
                var switchPosition = new[] { 0.0 };
                var speed = new[] { this.machineData.MessageData.TargetSpeed[0] / 2 };
                var newPositioningMessageData = new PositioningMessageData(
                    Axis.Horizontal,
                    MovementType.Relative,
                    MovementMode.FindZero,
                    -this.machineData.MessageData.TargetPosition / 2,
                    speed,
                    this.machineData.MessageData.TargetAcceleration,
                    this.machineData.MessageData.TargetDeceleration,
                    switchPosition,
                    HorizontalMovementDirection.Backwards);
                this.machineData.MessageData = newPositioningMessageData;
                // stop timers
                this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.stateData));
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

            this.Logger.LogDebug($"Continue Message send to inverter {this.machineData.CurrentInverterIndex}");
        }

        #endregion
    }
}
