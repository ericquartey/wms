using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionErrorState : StateBase
    {
        #region Constructors

        public MissionErrorState(IStateMachine parentMachine)
        {
            this.ParentStateMachine = parentMachine;

            //var newMessage = new CommandMessage(null,
            //    "Mission State Ending",
            //    MessageActor.Any,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.EndAction,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "MissionErrorState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action

                    var newMessage = new CommandMessage(null,
                        "Mission Error",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Stop,
                        MessageVerbosity.Info);
                    this.ParentStateMachine.PublishCommandMessage(newMessage);
                    break;
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
