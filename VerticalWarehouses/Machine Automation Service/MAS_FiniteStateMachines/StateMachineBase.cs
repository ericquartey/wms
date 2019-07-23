using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    public abstract class StateMachineBase : IStateMachine
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        protected StateMachineBase(IEventAggregator eventAggregator, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.Logger = logger;
            this.EventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~StateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected IState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void ChangeState(IState newState, CommandMessage message = null)
        {
            lock (this.CurrentState)
            {
                this.CurrentState = newState;
                this.CurrentState.Start();
            }

            this.Logger.LogTrace($"1:{newState.GetType()}");
            if (message != null)
            {
                this.Logger.LogTrace($"2:{newState.GetType()}{message.Type}:{message.Destination}");
                this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
            this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishFieldCommandMessage(FieldCommandMessage message)
        {
            this.Logger.LogTrace($"1:Publish Field Command: {message.Type}, destination: {message.Destination}, source: {message.Source}");
            this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Field Notification: {message.Type}, destination: {message.Destination}, source: {message.Source}");
            this.EventAggregator.GetEvent<FieldNotificationEvent>().Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification: {message.Type}, destination: {message.Destination}, source: {message.Source}");
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        /// <inheritdoc />
        public abstract void Start();

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
