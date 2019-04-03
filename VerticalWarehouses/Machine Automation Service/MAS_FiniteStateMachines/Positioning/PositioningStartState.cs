using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

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
            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.axisMovement = positioningMessageData.AxisMovement;

            //TEMP send a message to start the positioning (to inverter and other components)
            var newMessage = new CommandMessage(this.positioningMessageData,
                string.Format("Positioning {0} State Started", this.axisMovement),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageVerbosity.Info);
            this.ParentStateMachine.PublishCommandMessage(newMessage);
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
                    this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData));
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
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData));
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Change to error state when an error has occurred
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
