using System;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateMachineBase : IIoStateMachine
    {
        #region Fields

        protected IEventAggregator EventAggregator;

        protected BlockingConcurrentQueue<IoMessage> IoCommandQueue;

        protected ILogger Logger;

        private bool disposed;

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
            this.IoCommandQueue.Enqueue(message);
        }

        public virtual void ProcessMessage(IoMessage message)
        {
            this.CurrentState?.ProcessMessage(message);
        }

        public void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.Logger.LogTrace($"1:{notificationMessage.Type}:{notificationMessage.Destination}:{notificationMessage.Status}");

            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
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
