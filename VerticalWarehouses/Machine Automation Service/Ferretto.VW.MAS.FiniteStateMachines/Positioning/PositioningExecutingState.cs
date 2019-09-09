using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningExecutingState : StateBase
    {
        #region Fields

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        private Timer delayTimer;

        private bool disposed;

        private int numberExecutedSteps;

        private IPositioningFieldMessageData positioningDownFieldMessageData;

        private IPositioningFieldMessageData positioningUpFieldMessageData;

        #endregion

        #region Constructors

        public PositioningExecutingState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
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
                            this.ProcessExecutingStatusUpdate(message);
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
            if (this.machineData.MessageData.MovementMode == MovementMode.Position)
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

            if (this.machineData.MessageData.MovementMode == MovementMode.BeltBurnishing)
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

            if (this.machineData.MessageData.MovementMode == MovementMode.FindZero)
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

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            this.stateData.StopRequestReason = reason;
            this.machineData.ExecutedSteps = this.numberExecutedSteps;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void ProcessEndPositioning()
        {
            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.Position:
                    this.Logger.LogDebug("FSM Finished Executing State in Position Mode");
                    this.machineData.ExecutedSteps = this.numberExecutedSteps;
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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
                        // INFO Even to go Up and Odd for Down
                        var commandMessage = new FieldCommandMessage(
                           this.numberExecutedSteps % 2 == 0
                               ? this.positioningUpFieldMessageData
                               : this.positioningDownFieldMessageData,
                           $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                           FieldMessageActor.InverterDriver,
                           FieldMessageActor.FiniteStateMachines,
                           FieldMessageType.Positioning,
                           (byte)this.machineData.CurrentInverterIndex);

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
                            this.RequestingBay,
                            MessageStatus.OperationExecuting);

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
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
            if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
            {
                this.machineData.ExecutedSteps = this.numberExecutedSteps;
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
            }
            else
            {
                var newPositioningMessageData = new PositioningMessageData(
                    Axis.Horizontal,
                    MovementType.Relative,
                    MovementMode.FindZero,
                    -this.machineData.MessageData.TargetPosition / 2,
                    this.machineData.MessageData.TargetSpeed / 2,
                    this.machineData.MessageData.TargetAcceleration,
                    this.machineData.MessageData.TargetDeceleration,
                    0,
                    0,
                    0);
                this.machineData.MessageData = newPositioningMessageData;
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.stateData));
            }
        }

        private void ProcessExecutingStatusUpdate(FieldNotificationMessage message)
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.FindZero)
            {
                if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                {
                    var commandMessage = new FieldCommandMessage(
                        null,
                        $"Stop Operation due to zero position reached",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop,
                        (byte)this.machineData.CurrentInverterIndex);

                    this.Logger.LogTrace(
                        $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
                }
            }

            if (message.Data is InverterStatusUpdateFieldMessageData data)
            {
                this.machineData.MessageData.CurrentPosition = data.CurrentPosition;

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"Current Encoder position: {data.CurrentPosition}",
                    MessageActor.AutomationService,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    this.RequestingBay,
                    MessageStatus.OperationExecuting);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
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

        private void ProcessEndPositioning()
        {
            switch (this.positioningMessageData.MovementMode)
            {
                case MovementMode.Position:
                    this.Logger.LogDebug("FSM Finished Executing State in Position Mode");
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
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
            if (this.machineSensorsStatus.IsSensorZeroOnCradle)
            {
                this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps));
            }
            else
            {
                var newPositioningMessageData = new PositioningMessageData(
                    Axis.Horizontal,
                    MovementType.Relative,
                    MovementMode.FindZero,
                    -this.positioningMessageData.TargetPosition / 2,
                    this.positioningMessageData.TargetSpeed / 2,
                    this.positioningMessageData.TargetAcceleration,
                    this.positioningMessageData.TargetDeceleration,
                    0,
                    0,
                    0,
                    0);
                this.positioningMessageData = newPositioningMessageData;
                this.ParentStateMachine.ChangeState(new PositioningStartState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger));
            }
        }

        private void ProcessExecutingStatusUpdate(FieldNotificationMessage message)
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

            if (message.Data is InverterStatusUpdateFieldMessageData data)
            {
                this.positioningMessageData.CurrentPosition = data.CurrentPosition;

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

        #endregion
    }
}
