using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningExecutingState : StateBase
    {
        #region Fields

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private FieldCommandMessage commandMessage;

        private bool disposed;

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
            if (this.positioningMessageData.MovementMode == MovementMode.Position)
            {
                this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                this.commandMessage = new FieldCommandMessage(
                    this.positioningFieldMessageData,
                    $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            if (this.positioningMessageData.MovementMode == MovementMode.BeltBurnishing)
            {
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
                    this.positioningMessageData.UpperBound);

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
                    this.positioningMessageData.UpperBound);

                this.positioningUpFieldMessageData = new PositioningFieldMessageData(this.positioningUpMessageData);

                this.positioningDownFieldMessageData = new PositioningFieldMessageData(this.positioningDownMessageData);

                // TEMP Hypothesis: in the case of Belt Burninshing the first TargetPosition is the upper bound
                this.commandMessage = new FieldCommandMessage(
                    this.positioningUpFieldMessageData,
                    "Belt Burninshing Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            if (this.positioningMessageData.MovementMode == MovementMode.FindZero)
            {
                this.positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

                this.commandMessage = new FieldCommandMessage(
                    this.positioningFieldMessageData,
                    $"{this.positioningMessageData.AxisMovement} Positioning Find Zero Started",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }

            this.ParentStateMachine.PublishFieldCommandMessage(this.commandMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, this.Logger, this.numberExecutedSteps, true));
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
                        // INFO Even to go Up and Odd for Down
                        this.commandMessage = new FieldCommandMessage(
                            this.numberExecutedSteps % 2 == 0
                                ? this.positioningUpFieldMessageData
                                : this.positioningDownFieldMessageData,
                            $"Belt Burninshing moving cycle N° {this.numberExecutedSteps / 2}",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning);

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
                    this.commandMessage = new FieldCommandMessage(
                        null,
                        $"Stop Operation due to zero position reached",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop);

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
