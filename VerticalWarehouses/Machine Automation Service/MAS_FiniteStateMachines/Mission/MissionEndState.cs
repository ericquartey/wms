using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionEndState : StateBase
    {
        #region Constructors

        public MissionEndState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var newMessage = new NotificationMessage(null,
                "Mission State Ending",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.EndMission,
                MessageStatus.OperationEnd);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "MissionEndState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action

                    var newMessage = new CommandMessage(null,
                        "Stop Requested",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Stop,
                        MessageVerbosity.Info);
                    this.parentStateMachine.PublishCommandMessage(newMessage);
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
