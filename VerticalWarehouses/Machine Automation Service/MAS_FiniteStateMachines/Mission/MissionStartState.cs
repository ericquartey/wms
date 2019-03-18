using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionStartState : StateBase
    {
        #region Constructors

        public MissionStartState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var newMessage = new CommandMessage(null,
                "Mission State Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.StartAction,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);

            var inverterMessage = new CommandMessage(null,
                "Mission State Started",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StartAction,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "MissionStartState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action
                    this.ProcessStopAction(message);
                    break;

                case MessageType.EndAction:
                    //TODO add state business logic to stop current action
                    this.ProcessEndAction(message);
                    break;

                case MessageType.ErrorAction:
                    this.ProcessErrorAction(message);
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        private void ProcessEndAction(CommandMessage message)
        {
            var newMessage = new CommandMessage(null,
                "End Mission",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.EndAction,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionEndState(this.parentStateMachine), newMessage);
        }

        private void ProcessErrorAction(CommandMessage message)
        {
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionErrorState(this.parentStateMachine), newMessage);
        }

        private void ProcessStopAction(CommandMessage message)
        {
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionEndState(this.parentStateMachine), newMessage);
        }

        #endregion
    }
}
