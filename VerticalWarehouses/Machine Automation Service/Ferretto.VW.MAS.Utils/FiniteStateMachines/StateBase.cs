// ReSharper disable ArrangeThisQualifier
using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public abstract class StateBase : IState, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private bool hasEntered;

        private bool hasExited;

        private bool hasStopped;

        private bool isDisposed;

        private IServiceProvider serviceProvider;

        #endregion

        #region Constructors

        protected StateBase(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Properties

        protected ILogger<StateBase> Logger { get; }

        #endregion

        #region Methods

        public IState CommandReceived(CommandMessage commandMessage)
        {
            return this.OnCommandReceived(commandMessage);
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.OnDisposing();
                this.isDisposed = true;
            }
        }

        public void Enter(CommandMessage message, IServiceProvider serviceProvider, IFiniteStateMachineData machineData)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.serviceProvider = serviceProvider;

            if (this.hasEntered)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already entered.");
            }

            this.hasEntered = true;

            this.Logger.LogDebug($"Entering state {this.GetType()}.");

            this.OnEnter(message, machineData);
        }

        public void Exit()
        {
            if (this.hasExited)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already exited.");
            }

            this.hasExited = true;

            this.Logger.LogDebug($"Exiting state {this.GetType()}.");

            this.OnExit();
        }

        public virtual IState NotificationReceived(NotificationMessage notificationMessage)
        {
            return this.OnNotificationReceived(notificationMessage);
        }

        public IState Stop(StopRequestReason reason)
        {
            if (this.hasStopped)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already stopped.");
            }

            this.hasStopped = true;

            this.Logger.LogDebug($"Stopping state {this.GetType()}.");

            return this.OnStop(reason);
        }

        protected IState GetState<TState>()
            where TState : IState
        {
            return this.serviceProvider.GetRequiredService<TState>();
        }

        protected void NotifyCommandError(CommandMessage commandMessage, string description)
        {
            if (commandMessage is null)
            {
                throw new ArgumentNullException(nameof(commandMessage));
            }

            this.Logger.LogDebug($"Notifying Mission Manager service error caused by message {commandMessage.Type}");

            var message = new NotificationMessage(
                commandMessage.Data,
                description,
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.MissionManagerException,
                commandMessage.RequestingBay,
                commandMessage.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        protected virtual IState OnCommandReceived(CommandMessage commandMessage)
        {
            return this;
        }

        protected virtual void OnDisposing()
        {
            // do nothing
            // derived classes can customize the behaviour of this method
        }

        protected abstract void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData);

        protected virtual void OnExit()
        {
            // do nothing
        }

        protected virtual IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            return this;
        }

        protected abstract IState OnStop(StopRequestReason reason);

        #endregion
    }
}
