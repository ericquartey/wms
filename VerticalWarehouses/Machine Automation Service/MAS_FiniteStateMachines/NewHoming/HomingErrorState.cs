using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.NewHoming
{
    public class HomingErrorState : StateBase
    {
        #region Constructors

        public HomingErrorState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            // Notify the error condition
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

        public string Type => "MissionErrorState";

        #endregion

        #region Methods

        public override void MakeOperation()
        {
            throw new NotImplementedException();
        }

        public override void NotifyMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.StopAction:
                    //TODO add state business logic to stop current action

                    var newMessage = new CommandMessage(null,
                        "Mission Error",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.StopAction,
                        MessageVerbosity.Info);
                    this.parentStateMachine.PublishCommandMessage(newMessage);
                    break;
            }
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
