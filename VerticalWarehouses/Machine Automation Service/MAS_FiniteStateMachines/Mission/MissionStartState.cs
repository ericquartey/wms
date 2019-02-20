using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionStartState : StateBase
    {
        #region Constructors

        public MissionStartState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var newMessage = new Event_Message(null,
                $"Mission State Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageStatus.Start,
                MessageType.StartAction,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishMessage(newMessage);

            var inverterMessage = new Event_Message(null,
                $"Mission State Started",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageStatus.Start,
                MessageType.StartAction,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => $"MissionStartState";

        #endregion

        #region Methods

        public override void MakeOperation()
        {
            throw new System.NotImplementedException();
        }

        public override void NotifyMessage(Event_Message message)
        {
            switch (message.Type)
            {
                case MessageType.StopAction:
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

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        private void ProcessEndAction(Event_Message message)
        {
            var newMessage = new Event_Message(null,
                $"End Mission",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageStatus.End,
                MessageType.EndAction,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionEndState(this.parentStateMachine), newMessage);
        }

        private void ProcessErrorAction(Event_Message message)
        {
            var newMessage = new Event_Message(null,
                $"Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageStatus.End,
                MessageType.StopAction,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionErrorState(this.parentStateMachine), newMessage);
        }

        private void ProcessStopAction(Event_Message message)
        {
            var newMessage = new Event_Message(null,
                $"Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageStatus.End,
                MessageType.StopAction,
                MessageVerbosity.Info);
            this.parentStateMachine.ChangeState(new MissionEndState(this.parentStateMachine), newMessage);
        }

        #endregion
    }
}
