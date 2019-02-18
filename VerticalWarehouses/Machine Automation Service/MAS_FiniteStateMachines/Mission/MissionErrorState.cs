using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionErrorState : StateBase
    {
        #region Constructors

        public MissionErrorState( IStateMachine parentMachine )
        {
            this.parentStateMachine = parentMachine;

            Event_Message newMessage = new Event_Message( null,
                $"Mission State Ending",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageStatus.End,
                MessageType.EndAction,
                MessageVerbosity.Info );
            this.parentStateMachine.PublishMessage( newMessage );
        }

        #endregion

        #region Properties

        public string Type => $"MissionErrorState";

        #endregion

        #region Methods

        public override void NotifyMessage( Event_Message message )
        {
            switch(message.Type)
            {
                case MessageType.StopAction:
                    //TODO add state business logic to stop current action

                    Event_Message newMessage = new Event_Message( null,
                        $"Mission Error",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageStatus.End,
                        MessageType.StopAction,
                        MessageVerbosity.Info );
                    this.parentStateMachine.PublishMessage( newMessage );
                    break;
            }
        }

        #endregion
    }
}
