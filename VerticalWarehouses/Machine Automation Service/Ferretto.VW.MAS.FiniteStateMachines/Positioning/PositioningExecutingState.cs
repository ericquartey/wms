using System;
using System.Diagnostics;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    internal class PositioningExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly decimal fullPosition;

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        private Timer delayTimer;

        private bool isDisposed;

        private int numberExecutedSteps;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

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
        }

        #endregion

        #region Destructors

        ~PositioningExecutingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
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
            var inverterIndex = (this.machineData.MessageData.IsOneKMachine && this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                ? InverterIndex.Slave1
                : InverterIndex.MainInverter;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning,
                            (byte)this.machineData.CurrentInverterIndex);
                    }
                    break;

                case MovementMode.TorqueCurrentSampling:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData);
                        statusWordPollingInterval = 500;

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"Start torque current sampling",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.TorqueCurrentSampling,
                            (byte)inverterIndex);
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    {
                        // Build message for UP
                        var positioningUpMessageData = new PositioningMessageData(this.machineData.MessageData);
                        positioningUpMessageData.TargetPosition = positioningUpMessageData.UpperBound;

                        // Build message for DOWN
                        var positioningDownMessageData = new PositioningMessageData(this.machineData.MessageData);
                        positioningDownMessageData.TargetPosition = positioningDownMessageData.LowerBound;

                        this.positioningUpFieldMessageData = new PositioningFieldMessageData(positioningUpMessageData);

                        this.positioningDownFieldMessageData = new PositioningFieldMessageData(positioningDownMessageData);

                        // TEMP Hypothesis: in the case of Belt Burninshing the first TargetPosition is the upper bound
                        commandMessage = new FieldCommandMessage(
                            this.positioningUpFieldMessageData,
                            "Belt Burninshing Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning,
                            (byte)this.machineData.CurrentInverterIndex);
                    }
                    break;

                case MovementMode.FindZero:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning Find Zero Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning,
                            (byte)this.machineData.CurrentInverterIndex);
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
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex));
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            // stop timer
            this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;
            this.machineData.ExecutedSteps = this.numberExecutedSteps;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
            }

            this.isDisposed = true;
        }

        private void DelayElapsed(object state)
        {
            // INFO Even to go Up and Odd for Down
            var commandMessage = new FieldCommandMessage(
                this.numberExecutedSteps % 2 == 0
                    ? this.positioningUpFieldMessageData
                    : this.positioningDownFieldMessageData,
                $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace(
                $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var beltBurnishingPosition = this.numberExecutedSteps % 2 == 0
                ? BeltBurnishingPosition.LowerBound
                : BeltBurnishingPosition.UpperBound;

            this.machineData.MessageData.BeltBurnishingPosition = beltBurnishingPosition;

            // Notification message
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

            if (this.numberExecutedSteps > 0 &&
                this.numberExecutedSteps % 2 == 0)
            {
                using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                {
                    var setupStatusProvider = scope.ServiceProvider.GetRequiredService<ISetupStatusProvider>();

                    setupStatusProvider.IncreaseBeltBurnishingCycle();
                }
            }
        }

        private bool IsBracketSensorError()
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.Position
                && this.machineData.MessageData.MovementType == MovementType.Carousel)
            {
                return this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
            }
            return false;
        }

        private bool IsLoadingErrorDuringPickup()
        {
            return false;
            if (!this.machineData.MessageData.IsStartedOnBoard)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.machineData.MachineSensorStatus.AxisXPosition > this.machineData.MessageData.SwitchPosition[1]
                        && this.machineData.MachineSensorStatus.AxisXPosition < this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineData.MachineSensorStatus.AxisXPosition > this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.machineData.MachineSensorStatus.AxisXPosition < this.machineData.MessageData.SwitchPosition[1]
                        && this.machineData.MachineSensorStatus.AxisXPosition >= this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineData.MachineSensorStatus.AxisXPosition < this.fullPosition
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
            return false;
            if (this.machineData.MessageData.IsStartedOnBoard)
            {
                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.machineData.MachineSensorStatus.AxisXPosition > this.machineData.MessageData.SwitchPosition[1]
                        && this.machineData.MachineSensorStatus.AxisXPosition < this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineData.MachineSensorStatus.AxisXPosition > this.fullPosition
                        && !this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle)
                    {
                        return true;
                    }
                }
                else if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.machineData.MachineSensorStatus.AxisXPosition < this.machineData.MessageData.SwitchPosition[1]
                        && this.machineData.MachineSensorStatus.AxisXPosition >= this.machineData.MessageData.SwitchPosition[2]
                        && !this.machineData.MachineSensorStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineData.MachineSensorStatus.AxisXPosition < this.fullPosition
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
            return false;
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
                    var inverterIndex = (this.machineData.MessageData.IsOneKMachine && this.machineData.MessageData.AxisMovement == Axis.Horizontal) ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                    var commandMessage = new FieldCommandMessage(
                        null,
                        $"Stop Operation due to zero position reached",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
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
                    this.Logger.LogError("Cradle not correctly loaded during pickup");
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                }
                else if (this.IsUnloadingErrorDuringDeposit())
                {
                    this.Logger.LogError("Cradle not correctly unloaded during deposit");
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                }
            }

            if (message.Data is InverterStatusUpdateFieldMessageData data)
            {
                this.machineData.MessageData.CurrentPosition = data.CurrentPosition;
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
                    this.Logger.LogDebug("FSM Finished Executing State in Position Mode");
                    this.machineData.ExecutedSteps = this.numberExecutedSteps;
                    if (this.IsZeroSensorError())
                    {
                        this.Logger.LogError($"Zero sensor error after {(this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle ? "pickup" : "deposit")}");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                    }
                    else if (this.IsBracketSensorError())
                    {
                        this.Logger.LogError($"Bracket sensor error");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    this.numberExecutedSteps++;
                    this.machineData.MessageData.ExecutedCycles = this.numberExecutedSteps / 2;

                    if (this.numberExecutedSteps >= this.machineData.MessageData.NumberCycles * 2)
                    {
                        this.Logger.LogDebug("FSM Finished Executing State");
                        this.machineData.ExecutedSteps = this.numberExecutedSteps;
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
                    this.machineData.ExecutedSteps = this.numberExecutedSteps;
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle || this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
            {
                this.machineData.ExecutedSteps = this.numberExecutedSteps;
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
            }
            else
            {
                decimal[] switchPosition = { 0 };
                decimal[] speed = { this.machineData.MessageData.TargetSpeed[0] / 2 };
                var newPositioningMessageData = new PositioningMessageData(
                    Axis.Horizontal,
                    MovementType.Relative,
                    MovementMode.FindZero,
                    -this.machineData.MessageData.TargetPosition / 2,
                    speed,
                    this.machineData.MessageData.TargetAcceleration,
                    this.machineData.MessageData.TargetDeceleration,
                    0,
                    0,
                    0,
                    0,
                    switchPosition,
                    HorizontalMovementDirection.Backwards);
                this.machineData.MessageData = newPositioningMessageData;
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.stateData));
            }
        }

        #endregion
    }
}
