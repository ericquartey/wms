using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

        protected BlockingConcurrentQueue<IoWriteMessage> IoCommandQueue;

        private bool isDisposed;

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

        #region Properties

        protected IIoState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            if (newState is null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.IoDriver, newState.GetType().Name, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"IoDriver current state {newState.GetType().Name}",
                MessageActor.Any,
                MessageActor.IoDriver,
                MessageType.MachineStateActive,
                MessageStatus.OperationStart);

            this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            if (this.CurrentState is IDisposable disposableState)
            {
                disposableState.Dispose();
            }

            this.CurrentState = newState;
            this.CurrentState.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void EnqueueMessage(IoWriteMessage message)
        {
            this.IoCommandQueue.Enqueue(message);
        }

        public virtual void ProcessMessage(IoMessage message)
        {
            this.CurrentState?.ProcessMessage(message);
        }

        public virtual void ProcessResponseMessage(IoReadMessage message)
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
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.IoDriver, string.Empty, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"IoDriver current state null",
                MessageActor.Any,
                MessageActor.IoDriver,
                MessageType.MachineStateActive,
                MessageStatus.OperationStart);

            this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            this.isDisposed = true;
        }

        #endregion
    }
}
