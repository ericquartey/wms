using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateMachineBase : IIoStateMachine
    {
        #region Fields

        protected IEventAggregator eventAggregator;

        protected BlockingConcurrentQueue<IoMessage> ioCommandQueue;

        #endregion

        #region Properties

        protected IIoState CurrentState { get; set; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            this.CurrentState = newState;
        }

        public void EnqueueMessage(IoMessage message)
        {
            this.ioCommandQueue.Enqueue(message);
        }

        public virtual void ProcessMessage(IoMessage message)
        {
            this.CurrentState?.ProcessMessage(message);
        }

        public void PublishNotificationEvent(NotificationMessage notificationMessage)
        {
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        public abstract void Start();

        #endregion
    }
}
