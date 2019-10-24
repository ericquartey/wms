﻿using System;
using System.Diagnostics;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly IBaysProvider baysProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly double fullPosition;

        private readonly IPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IPositioningStateData stateData;

        private bool beltBurnishingMovingToInitialPosition;

        private bool beltBurnishingMovingUpwards;

        private Timer delayTimer;

        private bool isDisposed;

        private int performedCycles;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private bool profileStarted;

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
                this.fullPosition = this.machineData.MessageData.SwitchPosition[3];
                this.fullPosition += (this.machineData.MessageData.SwitchPosition[4] - this.machineData.MessageData.SwitchPosition[3]) / 2;
            }

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysProvider = this.scope.ServiceProvider.GetRequiredService<IBaysProvider>();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
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
                            this.ProcessEndPositioning();
                            break;

                        case FieldMessageType.InverterStop:
                            this.ProcessEndStop();
                            break;

                        case FieldMessageType.MeasureProfile:
                            this.profileStarted = true;
                            break;
                    }
                    break;

                case MessageStatus.OperationError:
                    this.stateData.FieldMessage = message;
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
            FieldCommandMessage commandMessage = null;
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                case MovementMode.PositionAndMeasure:
                case MovementMode.BayChain:
                case MovementMode.BayChainManual:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            (byte)this.machineData.CurrentInverterIndex);

                        if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasure &&
                            this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                        {
                            var ioCommandMessageData = new MeasureProfileFieldMessageData(true);
                            var ioCommandMessage = new FieldCommandMessage(
                                ioCommandMessageData,
                                $"Measure Profile Start ",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.MeasureProfile,
                                (byte)this.baysProvider.GetIoDevice(this.machineData.RequestingBay));

                            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
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
            this.Logger.LogDebug("1:Stop Method Start");

            // stop timer
            this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;
            this.machineData.ExecutedSteps = this.performedCycles;
            if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasure &&
                this.machineData.MessageData.AxisMovement == Axis.Horizontal)
            {
                var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Measure Profile Stop ",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.MeasureProfile,
                    (byte)this.baysProvider.GetIoDevice(this.machineData.RequestingBay));

                this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
            }
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"Current position {beltBurnishingPosition}",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            if (!this.beltBurnishingMovingUpwards && !this.beltBurnishingMovingToInitialPosition)
            {
                var procedure = this.setupProceduresDataProvider.GetBeltBurnishingTest();
                this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
            }
        }

        private bool IsBracketSensorError()
        {
            return !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
        }

        private bool IsLoadingErrorDuringPickup()
        {
            if (!this.machineData.MessageData.IsStartedOnBoard)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.elevatorProvider.HorizontalPosition > this.machineData.MessageData.SwitchPosition[1]
                        && this.elevatorProvider.HorizontalPosition < this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.elevatorProvider.HorizontalPosition > this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.elevatorProvider.HorizontalPosition < this.machineData.MessageData.SwitchPosition[1]
                        && this.elevatorProvider.HorizontalPosition >= this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.elevatorProvider.HorizontalPosition < this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsUnloadingErrorDuringDeposit()
        {
            if (this.machineData.MessageData.IsStartedOnBoard)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.elevatorProvider.HorizontalPosition > this.machineData.MessageData.SwitchPosition[1]
                        && this.elevatorProvider.HorizontalPosition < this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.elevatorProvider.HorizontalPosition > this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle)
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.elevatorProvider.HorizontalPosition < this.machineData.MessageData.SwitchPosition[1]
                        && this.elevatorProvider.HorizontalPosition >= this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.elevatorProvider.HorizontalPosition < this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsZeroSensorError()
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.Position
                && this.machineData.MessageData.MovementType == MovementType.TableTarget
                && this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle == this.machineData.MachineSensorStatus.IsSensorZeroOnCradle
                )
            {
                return true;
            }
            return false;
        }

        private void OnInverterStatusUpdated(FieldNotificationMessage message)
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.FindZero)
            {
                if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                {
                    var inverterIndex = this.machineData.CurrentInverterIndex;
                    var commandMessage = new FieldCommandMessage(
                        null,
                        $"Stop Operation due to zero position reached",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.InverterStop,
                        (byte)inverterIndex);

                    this.Logger.LogTrace(
                        $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
                }
            }
            else if (this.machineData.MessageData.MovementMode == MovementMode.Position && this.machineData.MessageData.MovementType == MovementType.TableTarget)
            {
                if (this.IsLoadingErrorDuringPickup())
                {
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);

                    this.Logger.LogError("Cradle not correctly loaded during pickup");
                    this.stateData.FieldMessage = message;
                    this.Stop(StopRequestReason.Stop);
                }
                else if (this.IsUnloadingErrorDuringDeposit())
                {
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
                    this.Logger.LogError("Cradle not correctly unloaded during deposit");
                    this.stateData.FieldMessage = message;
                    this.Stop(StopRequestReason.Stop);
                }
            }

            if (message.Data is InverterStatusUpdateFieldMessageData data)
            {
                if (data.CurrentPosition != null)
                {
                    this.machineData.MessageData.CurrentPosition = data.CurrentPosition;
                }
                this.machineData.MessageData.TorqueCurrentSample = data.TorqueCurrent;

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"Current Encoder position: {data.CurrentPosition}",
                    MessageActor.AutomationService,
                    MessageActor.FiniteStateMachines,
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
                case MovementMode.PositionAndMeasure:
                    this.Logger.LogDebug($"FSM Finished Executing State in {this.machineData.MessageData.MovementMode} Mode");
                    this.machineData.ExecutedSteps = this.performedCycles;
                    if (this.IsZeroSensorError())
                    {
                        if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                        {
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.ZeroSensorErrorAfterPickup, this.machineData.RequestingBay);
                            this.Logger.LogError($"Zero sensor error after pickup");
                        }
                        else
                        {
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.ZeroSensorErrorAfterDeposit, this.machineData.RequestingBay);
                            this.Logger.LogError($"Zero sensor error after deposit");
                        }
                        this.Stop(StopRequestReason.Stop);
                    }
                    else
                    {
                        if (this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasure &&
                            this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                        {
                            var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
                            var ioCommandMessage = new FieldCommandMessage(
                                ioCommandMessageData,
                                $"Measure Profile Stop ",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.DeviceManager,
                                FieldMessageType.MeasureProfile,
                                (byte)this.baysProvider.GetIoDevice(this.machineData.RequestingBay));

                            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                            this.ParentStateMachine.ChangeState(new PositioningProfileState(this.stateData));
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                        }
                    }
                    break;

                case MovementMode.BeltBurnishing:
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
                    break;

                case MovementMode.FindZero:
                    this.machineData.ExecutedSteps = this.performedCycles;
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    break;

                case MovementMode.BayChain:
                    if (this.IsBracketSensorError())
                    {
                        this.Logger.LogError($"Bracket sensor error");
                        this.Stop(StopRequestReason.Stop);
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    }
                    break;

                case MovementMode.BayChainManual:
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.stateData));
            }
        }

        #endregion
    }
}
