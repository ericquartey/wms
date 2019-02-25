using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        void ChangeState( IState newState, Event_Message message = null );

        void NotifyMessage( Event_Message message );

        void PublishMessage( Event_Message message );

        void Start();

        #endregion
    }
}
