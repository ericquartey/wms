using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownErrorState : StateBase
    {
        #region Constructors

        public UpDownErrorState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            //TEMP Notify the error condition
            var newMessage = new NotificationMessage(null,
                "Up&Down Error State",
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

        public override string Type => "UpDownErrorState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
