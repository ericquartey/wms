using System;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines
{
    public abstract class IoStateMachineBase : IIoStateMachine
    {
        #region Fields

        protected BlockingConcurrentQueue<IoSHDWriteMessage> IoCommandQueue;

        private bool disposed;

        #endregion

        #region Constructors

        public IoStateMachineBase(
            IEventAggregator eventAggregator,
            ILogger logger)
        {
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
        }

        #endregion

        #region Destructors

        ~IoStateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected IIoState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            this.CurrentState.Dispose();

            this.CurrentState = newState;
            this.CurrentState.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EnqueueMessage(IoSHDWriteMessage message)
        {
            this.IoCommandQueue.Enqueue(message);
        }

        public virtual void ProcessMessage(IoSHDMessage message)
        {
            this.CurrentState?.ProcessMessage(message);
        }

        public virtual void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.CurrentState?.ProcessResponseMessage(message);
        }

        public void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

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
