using System;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        protected IEventAggregator eventAggregator;

        protected BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private bool disposed = false;

        #endregion

        #region Destructors

        ~InverterStateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected IInverterState CurrentState { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void ChangeState(IInverterState newState)
        {
            this.CurrentState = newState;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void EnqueueMessage(InverterMessage message)
        {
            this.inverterCommandQueue.Enqueue(message);
        }

        public abstract void OnPublishNotification(NotificationMessage message);

        /// <inheritdoc />
        public bool ProcessMessage(InverterMessage message)
        {
            return this.CurrentState?.ProcessMessage(message) ?? false;
        }

        /// <inheritdoc />
        public void PublishNotificationEvent(NotificationMessage notificationMessage)
        {
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public abstract void Stop();

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
