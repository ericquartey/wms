using System;
using System.Diagnostics;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
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

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private FieldCommandMessage commandMessage;

        private Timer delayTimer;

        private bool isDisposed;

        private int numberExecutedSteps;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningMessageData positioningDownMessageData;

        private IPositioningFieldMessageData positioningFieldMessageData;

        private IPositioningMessageData positioningMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        private IPositioningMessageData positioningUpMessageData;

        #endregion

        #region Constructors

        public PositioningExecutingState(
            IStateMachine parentMachine,
            IMachineSensorsStatus machineSensorsStatus,
            IPositioningMessageData positioningMessageData,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.positioningMessageData = positioningMessageData;
            this.machineSensorsStatus = machineSensorsStatus;

            if (this.positioningMessageData.MovementMode == MovementMode.Position
                &&
                this.positioningMessageData.MovementType == MovementType.TableTarget)
            {
                this.fullPosition = this.positioningMessageData.SwitchPosition[3];
                this.fullPosition += (this.positioningMessageData.SwitchPosition[4] - this.positioningMessageData.SwitchPosition[3]) / 2;
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

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
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, message, this.Logger));
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = (this.positioningMessageData.IsOneKMachine && this.positioningMessageData.AxisMovement == Axis.Horizontal)
                ? InverterIndex.Slave1
                : InverterIndex.MainInverter;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;

            switch (this.positioningMessageData.MovementMode)
            {
                case MovementMode.Position:
                    this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                    this.commandMessage = new FieldCommandMessage(
                        this.positioningFieldMessageData,
                        $"Start {this.positioningMessageData.AxisMovement} positioning",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.Positioning,
                        (byte)inverterIndex);
                    break;

                case MovementMode.TorqueCurrentSampling:
                    this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);
                    statusWordPollingInterval = 500;

                    this.commandMessage = new FieldCommandMessage(
                        this.positioningFieldMessageData,
                        $"Start torque current sampling",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.TorqueCurrentSampling,
                        (byte)inverterIndex);
                    break;

                case MovementMode.BeltBurnishing:
                    // Build message for UP
                    this.positioningUpMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        this.positioningMessageData.MovementMode,
                        this.positioningMessageData.UpperBound,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound,
                        this.positioningMessageData.Delay,
                        this.positioningMessageData.SwitchPosition,
                        this.positioningMessageData.Direction);

                    // Build message for DOWN
                    this.positioningDownMessageData = new PositioningMessageData(
                        this.positioningMessageData.AxisMovement,
                        this.positioningMessageData.MovementType,
                        this.positioningMessageData.MovementMode,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.TargetSpeed,
                        this.positioningMessageData.TargetAcceleration,
                        this.positioningMessageData.TargetDeceleration,
                        this.positioningMessageData.NumberCycles,
                        this.positioningMessageData.LowerBound,
                        this.positioningMessageData.UpperBound,
                        this.positioningMessageData.Delay,
                        this.positioningMessageData.SwitchPosition,
                        this.positioningMessageData.Direction);

                    this.positioningUpFieldMessageData = new PositioningFieldMessageData(this.positioningUpMessageData);

                    this.positioningDownFieldMessageData = new PositioningFieldMessageData(this.positioningDownMessageData);

                    // TEMP Hypothesis: in the case of Belt Burninshing the first TargetPosition is the upper bound
                    this.commandMessage = new FieldCommandMessage(
                        this.positioningUpFieldMessageData,
                        "Belt Burninshing Started",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.Positioning,
                        (byte)InverterIndex.MainInverter);
                    break;

                case MovementMode.FindZero:
                    this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                    this.commandMessage = new FieldCommandMessage(
                        this.positioningFieldMessageData,
                        $"{this.positioningMessageData.AxisMovement} Positioning Find Zero Started",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.Positioning,
                        (byte)inverterIndex);
                    break;
            }

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, statusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex));
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            // stop timer
            this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.ParentStateMachine.ChangeState(
                new PositioningEndState(
                    this.ParentStateMachine,
                    this.machineSensorsStatus,
                    this.positioningMessageData,
                    this.Logger,
                    this.numberExecutedSteps,
                    true));
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
            this.commandMessage = new FieldCommandMessage(
                this.numberExecutedSteps % 2 == 0
                    ? this.positioningUpFieldMessageData
                    : this.positioningDownFieldMessageData,
                $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace(
                $"2:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);

            var beltBurnishingPosition = this.numberExecutedSteps % 2 == 0
                ? BeltBurnishingPosition.LowerBound
                : BeltBurnishingPosition.UpperBound;

            this.positioningMessageData.BeltBurnishingPosition = beltBurnishingPosition;

            // Notification message
            var notificationMessage = new NotificationMessage(
                this.positioningMessageData,
                $"Current position {beltBurnishingPosition}",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
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

                Debug.Write("Belt completed cycle.");
            }

            Debug.Write("Belt current position " + beltBurnishingPosition);
        }

        private bool IsLoadingErrorDuringPickup()
        {
            if (!this.positioningMessageData.IsStartedOnBoard)
            {
                if (this.positioningMessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.machineSensorsStatus.AxisXPosition > this.positioningMessageData.SwitchPosition[1]
                        && this.machineSensorsStatus.AxisXPosition < this.positioningMessageData.SwitchPosition[2]
                        && !this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineSensorsStatus.AxisXPosition > this.fullPosition
                        && !this.machineSensorsStatus.IsDrawerCompletelyOnCradle)
                    {
                        return true;
                    }
                }
                else if (this.positioningMessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.machineSensorsStatus.AxisXPosition < this.positioningMessageData.SwitchPosition[1]
                        && this.machineSensorsStatus.AxisXPosition >= this.positioningMessageData.SwitchPosition[2]
                        && !this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineSensorsStatus.AxisXPosition < this.fullPosition
                        && !this.machineSensorsStatus.IsDrawerCompletelyOnCradle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsUnloadingErrorDuringDeposit()
        {
            if (this.positioningMessageData.IsStartedOnBoard)
            {
                if (this.positioningMessageData.Direction == HorizontalMovementDirection.Forwards)
                {
                    if (this.machineSensorsStatus.AxisXPosition > this.positioningMessageData.SwitchPosition[1]
                        && this.machineSensorsStatus.AxisXPosition < this.positioningMessageData.SwitchPosition[2]
                        && !this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineSensorsStatus.AxisXPosition > this.fullPosition
                        && !this.machineSensorsStatus.IsDrawerCompletelyOffCradle)
                    {
                        return true;
                    }
                }
                else if (this.positioningMessageData.Direction == HorizontalMovementDirection.Backwards)
                {
                    if (this.machineSensorsStatus.AxisXPosition < this.positioningMessageData.SwitchPosition[1]
                        && this.machineSensorsStatus.AxisXPosition >= this.positioningMessageData.SwitchPosition[2]
                        && !this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1
                        )
                    {
                        return true;
                    }
                    if (this.machineSensorsStatus.AxisXPosition < this.fullPosition
                        && !this.machineSensorsStatus.IsDrawerCompletelyOffCradle)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsZeroSensorError()
        {
            return this.positioningMessageData.MovementMode == MovementMode.Position
                && this.positioningMessageData.MovementType == MovementType.TableTarget
                && this.machineSensorsStatus.IsDrawerCompletelyOnCradle == this.machineSensorsStatus.IsSensorZeroOnCradle;
        }

        private void OnInverterStatusUpdated(FieldNotificationMessage message)
        {
            if (this.positioningMessageData.MovementMode == MovementMode.FindZero)
            {
                if (this.machineSensorsStatus.IsSensorZeroOnCradle)
                {
                    var inverterIndex = (this.positioningMessageData.IsOneKMachine && this.positioningMessageData.AxisMovement == Axis.Horizontal) ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                    this.commandMessage = new FieldCommandMessage(
                        null,
                        $"Stop Operation due to zero position reached",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop,
                        (byte)inverterIndex);

                    this.Logger.LogTrace(
                        $"2:Publishing Field Command Message {this.commandMessage.Type} Destination {this.commandMessage.Destination}");

                    this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);
                }
            }
            else if (this.positioningMessageData.MovementMode == MovementMode.Position && this.positioningMessageData.MovementType == MovementType.TableTarget)
            {
                if (this.IsLoadingErrorDuringPickup())
                {
                    this.Logger.LogError("Cradle not correctly loaded during pickup");
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, message, this.Logger));
                }
                else if (this.IsUnloadingErrorDuringDeposit())
                {
                    this.Logger.LogError("Cradle not correctly unloaded during deposit");
                    this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, message, this.Logger));
                }
            }

            if (message.Data is InverterStatusUpdateFieldMessageData data)
            {
                this.positioningMessageData.CurrentPosition = data.CurrentPosition;
                this.positioningMessageData.TorqueCurrentSample = data.TorqueCurrent;

                var notificationMessage = new NotificationMessage(
                    this.positioningMessageData,
                    $"Current Encoder position: {data.CurrentPosition}",
                    MessageActor.AutomationService,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationExecuting);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        private void ProcessEndPositioning()
        {
            switch (this.positioningMessageData.MovementMode)
            {
                case MovementMode.Position:
                    this.Logger.LogDebug("FSM Finished Executing State in Position Mode");
                    if (this.IsZeroSensorError())
                    {
                        this.Logger.LogError($"Zero sensor error after {(this.machineSensorsStatus.IsDrawerCompletelyOnCradle ? "pickup" : "deposit")}");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, null, this.Logger));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
                    }
                    break;

                case MovementMode.BeltBurnishing:
                    this.numberExecutedSteps++;
                    this.positioningMessageData.ExecutedCycles = this.numberExecutedSteps / 2;

                    if (this.numberExecutedSteps >= this.positioningMessageData.NumberCycles * 2)
                    {
                        this.Logger.LogDebug("FSM Finished Executing State");
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
                    }
                    else
                    {
                        if (this.positioningMessageData.Delay > 0)
                        {
                            this.delayTimer = new Timer(this.DelayElapsed, null, this.positioningMessageData.Delay * 1000, Timeout.Infinite);
                        }
                        else
                        {
                            this.DelayElapsed(null);
                        }
                    }
                    break;

                case MovementMode.FindZero:
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (this.machineSensorsStatus.IsSensorZeroOnCradle || this.machineSensorsStatus.IsDrawerCompletelyOnCradle)
            {
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
            }
            else
            {
                decimal[] switchPosition = { 0 };
                decimal[] speed = { this.positioningMessageData.TargetSpeed[0] / 2 };
                var newPositioningMessageData = new PositioningMessageData(
                    Axis.Horizontal,
                    MovementType.Relative,
                    MovementMode.FindZero,
                    -this.positioningMessageData.TargetPosition / 2,
                    speed,
                    this.positioningMessageData.TargetAcceleration,
                    this.positioningMessageData.TargetDeceleration,
                    0,
                    0,
                    0,
                    0,
                    switchPosition,
                    HorizontalMovementDirection.Backwards);
                this.positioningMessageData = newPositioningMessageData;
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger));
            }
        }

        #endregion
    }
}
