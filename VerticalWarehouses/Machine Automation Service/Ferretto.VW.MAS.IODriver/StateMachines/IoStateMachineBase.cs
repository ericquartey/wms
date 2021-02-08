using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines
{
    internal abstract class IoStateMachineBase : IIoStateMachine
    {
        #region Fields

        private readonly object lockObj = new object();

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isDisposed;

        #endregion

        #region Constructors

        public IoStateMachineBase(
            IEventAggregator eventAggregator,
            ILogger logger,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.IoCommandQueue = ioCommandQueue ?? throw new ArgumentNullException(nameof(ioCommandQueue));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        #endregion

        #region Properties

        protected IIoState CurrentState { get; private set; }

        protected IEventAggregator EventAggregator { get; }

        protected BlockingConcurrentQueue<IoWriteMessage> IoCommandQueue { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            if (newState is null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            lock (this.lockObj)
            {
                var notificationMessageData = new MachineStateActiveMessageData(MessageActor.IoDriver, newState.GetType().Name, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"IoDriver current state {newState.GetType().Name}",
                    MessageActor.Any,
                    MessageActor.IoDriver,
                    MessageType.MachineStateActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                if (this.CurrentState is IDisposable disposableState)
                {
                    disposableState.Dispose();
                }

                this.CurrentState = newState;
                newState.Start();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void EnqueueMessage(IoWriteMessage message)
        {
            this.IoCommandQueue.Enqueue(message);
        }

        public TService GetRequiredService<TService>()
            where TService : class
        {
            return this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<TService>();
        }

        public virtual void ProcessResponseMessage(IoReadMessage message)
        {
            lock (this.lockObj)
            {
                this.CurrentState?.ProcessResponseMessage(message);
            }
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

            this.isDisposed = true;
        }

        #endregion
    }
}
