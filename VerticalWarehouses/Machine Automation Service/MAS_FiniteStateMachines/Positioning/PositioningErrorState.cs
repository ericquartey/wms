using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.axisMovement = positioningMessageData.AxisMovement;

            //TEMP Notify the error condition
            var newMessage = new NotificationMessage(null,
                string.Format("Positioning {0} Error State", this.axisMovement),
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => string.Format("PositioningErrorState {0}", this.axisMovement);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            //TEMP Add your implementation code here
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //TEMP Add your implememtation code here
        }

        #endregion
    }
}
