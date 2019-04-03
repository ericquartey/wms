using System;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        protected IEventAggregator EventAggregator;

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private bool disposed;

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
            this.InverterCommandQueue.Enqueue(message);
        }

        /// <inheritdoc />
        public bool ProcessMessage(InverterMessage message)
        {
            return this.CurrentState?.ProcessMessage(message) ?? false;
        }

        /// <inheritdoc />
        public virtual void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
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
