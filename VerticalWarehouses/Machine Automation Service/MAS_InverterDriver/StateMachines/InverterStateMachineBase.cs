using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        protected IEventAggregator eventAggregator;

        protected BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        #endregion

        #region Properties

        protected IInverterState CurrentState { get; set; }

        #endregion

        #region Methods

        public virtual void ChangeState(IInverterState newState)
        {
            this.CurrentState = newState;
        }

        public void EnqueueMessage(InverterMessage message)
        {
            this.inverterCommandQueue.Enqueue(message);
        }

        public void NotifyMessage(InverterMessage message)
        {
            this.CurrentState?.NotifyMessage(message);
        }

        public void PublishNotificationEvent(NotificationMessage notificationMessage)
        {
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        public abstract void Start();

        #endregion
    }
}
