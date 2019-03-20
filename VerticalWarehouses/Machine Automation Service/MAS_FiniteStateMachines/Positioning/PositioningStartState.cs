using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStartState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningStartState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.axisMovement = positioningMessageData.AxisMovement;

            //TEMP send a message to start the positioning (to inverter and other components)
            var newMessage = new CommandMessage(this.positioningMessageData,
                string.Format("Positioning {0} State Started", this.axisMovement),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "PositioningStartState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Change to positioning end state (a request of stop operation has been made)
                    this.parentStateMachine.ChangeState(new PositioningEndState(this.parentStateMachine, this.positioningMessageData));
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
                        //TEMP Change to positioning end state after the positioning is done successfully
                        this.parentStateMachine.ChangeState(new PositioningEndState(this.parentStateMachine, this.positioningMessageData));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state when an error has occurred
                        this.parentStateMachine.ChangeState(new PositioningErrorState(this.parentStateMachine, this.positioningMessageData));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
