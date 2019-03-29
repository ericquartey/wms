using System;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateMachineBase : IIoStateMachine, IDisposable
    {
        #region Fields

        protected IEventAggregator eventAggregator;

        protected BlockingConcurrentQueue<IoMessage> ioCommandQueue;

        protected ILogger logger;

        private bool disposed = false;

        #endregion

        #region Destructors

        ~IoStateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected IIoState CurrentState { get; set; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            this.CurrentState.Dispose();
            this.CurrentState = newState;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
            this.logger.LogTrace(string.Format("1:{0}:{1}:{2}",
                notificationMessage.Type,
                notificationMessage.Destination,
                notificationMessage.Status));

            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        public abstract void Start();

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
        }

        #endregion
    }
}
