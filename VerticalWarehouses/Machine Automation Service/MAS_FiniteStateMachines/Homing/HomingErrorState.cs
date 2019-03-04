using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingErrorState : StateBase
    {
        #region Constructors

        public HomingErrorState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            //TEMP Notify the error condition
            var newMessage = new NotificationMessage(null,
                "Error Homing State",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                MessageStatus.OperationError,
                ErrorLevel.Error,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingErrorState";

        #endregion

        #region Methods

        public override void SendCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void SendNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
