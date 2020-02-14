using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.DeviceManager
{
    internal abstract class StateMachineBase : IStateMachine
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        protected StateMachineBase(
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.CurrentState = new EmptyState(this.Logger);
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator { get; }

        public ILogger Logger { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        protected IState CurrentState { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void ChangeState(IState newState, CommandMessage message = null)
        {
            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.DeviceManager, newState.GetType().Name, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"FSM current state {newState.GetType().Name}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.MachineStateActive,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationStart);

            this.PublishNotificationMessage(notificationMessage);

            if (this.CurrentState is IDisposable disposableState)
            {
                disposableState.Dispose();
            }

            lock (this.CurrentState)
            {
                this.CurrentState = newState;
                newState.Start();
            }

            this.Logger.LogTrace($"1:{newState.GetType().Name}");
            if (message != null)
            {
                this.Logger.LogTrace($"2:{newState.GetType().Name}{message.Type}:{message.Destination}");
                this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <inheritdoc />
        public abstract void ProcessCommandMessage(CommandMessage message);

        /// <inheritdoc />
        public abstract void ProcessFieldNotificationMessage(FieldNotificationMessage message);

        /// <inheritdoc />
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        /// <inheritdoc />
        public virtual void PublishCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Publish Command: {message.Type}, destination: {message.Destination}, source: {message.Source}");

            this.EventAggregator
                .GetEvent<CommandEvent>()
                .Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishFieldCommandMessage(FieldCommandMessage message)
        {
            this.Logger.LogTrace($"1:Publish Field Command: {message.Type}, destination: {message.Destination}, source: {message.Source}");

            this.EventAggregator
                .GetEvent<FieldCommandEvent>()
                .Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Field Notification: {message.Type}, destination: {message.Destination}, source: {message.Source}");

            this.EventAggregator
                .GetEvent<FieldNotificationEvent>()
                .Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification: {message.Type}, destination: {message.Destination}, source: {message.Source}");

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        /// <inheritdoc />
        public abstract void Start();

        public abstract void Stop(StopRequestReason reason);

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
