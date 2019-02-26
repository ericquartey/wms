using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        void ChangeState(IState newState, CommandMessage message = null);

        void NotifyMessage(CommandMessage message);

        void PublishCommandMessage(CommandMessage message);

        void PublishNotificationMessage(NotificationMessage message);

        void Start();

        #endregion
    }
}
