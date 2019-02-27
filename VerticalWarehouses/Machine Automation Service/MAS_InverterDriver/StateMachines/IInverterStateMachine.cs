using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.InverterDriver.StateMachines
{
    public interface IInverterStateMachine
    {
        #region Methods

        void ChangeState(IInverterState newState);

        void EnqueueMessage(InverterMessage message);

        void NotifyMessage(InverterMessage message);

        void PublishNotificationEvent(NotificationMessage notificationMessage);

        void Start();

        #endregion
    }
}
