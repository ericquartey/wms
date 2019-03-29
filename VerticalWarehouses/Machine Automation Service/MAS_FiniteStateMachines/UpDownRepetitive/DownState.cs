using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class DownState : StateBase
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        #endregion

        #region Constructors

        public DownState(IStateMachine parentMachine, IUpDownRepetitiveMessageData upDownMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.upDownMessageData = upDownMessageData;

            //TEMP Send a notification message about the progress of procedure
            var notifyMessage = new NotificationMessage(null,
                "Up&Down running",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.UpDown,
                MessageStatus.OperationExecuting);
            this.parentStateMachine.OnPublishNotification(notifyMessage);

            var target = this.upDownMessageData.TargetUpperBound;
            //TEMP Values are retrieve by the DataLayer i.e. var speed = this.data.GetSpeedValue();
            var speed = 0.0m;
            var acceleration = 0.0m;
            var deceleration = 0.0m;

            var positioningData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, target, speed, acceleration, deceleration);
            //TEMP send a message to start the positioning (to inverter and other components) toward upper position
            var commandMessage = new CommandMessage(positioningData,
                "Positioning Up",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(commandMessage);
        }

        #endregion

        #region Properties

        public override string Type => "DownState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action
                    this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine, this.upDownMessageData));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Change to the Up state (the positioning operation has been done successfully)
                        this.parentStateMachine.ChangeState(new UpState(this.parentStateMachine, this.upDownMessageData));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP an error occurs
                        this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine, this.upDownMessageData));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
