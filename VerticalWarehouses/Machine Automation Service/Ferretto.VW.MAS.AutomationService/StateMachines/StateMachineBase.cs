using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines
{
    public abstract class StateMachineBase : IStateMachine
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        protected StateMachineBase(
            BayNumber requestingBay,
            IEventAggregator eventAggregator,
            ILogger<AutomationService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.RequestingBay = requestingBay;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator)); ;
        }

        #endregion

        #region Destructors

        ~StateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator { get; }

        public ILogger<AutomationService> Logger { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        protected IState CurrentState { get; set; }

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
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        /// <inheritdoc />
        public virtual void PublishCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Publish Command: {message.Type}, destination: {message.Destination}, source: {message.Source}");
            this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        /// <inheritdoc />
        public virtual void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification: {message.Type}, destination: {message.Destination}, source: {message.Source}");
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(message);
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
