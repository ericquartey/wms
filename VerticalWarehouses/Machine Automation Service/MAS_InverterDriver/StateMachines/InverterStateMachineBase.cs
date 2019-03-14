using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
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
            Dispose(false);
        }

        #endregion

        #region Properties

        protected IInverterState CurrentState { get; set; }

        #endregion

        #region Methods

        public virtual void ChangeState(IInverterState newState)
        {
            this.CurrentState = newState;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EnqueueMessage(InverterMessage message)
        {
            this.inverterCommandQueue.Enqueue(message);
        }

        public bool ProcessMessage(InverterMessage message)
        {
            return this.CurrentState?.ProcessMessage(message) ?? false;
        }

        public void PublishNotificationEvent(NotificationMessage notificationMessage)
        {
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
        }

        public abstract void Start();

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        #endregion
    }
}
