// ReSharper disable ArrangeThisQualifier
using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public abstract class StateBase : IState
    {
        #region Fields

        private bool hasEntered;

        private bool hasExited;

        private bool hasStopped;

        private IServiceProvider serviceProvider;

        #endregion

        #region Constructors

        protected StateBase(
            ILogger<StateBase> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        protected ILogger<StateBase> Logger { get; }

        #endregion

        #region Methods

        public IState Abort()
        {
            this.Logger.LogDebug($"Aborting state {this.GetType().Name}.");

            return this.OnAbort();
        }

        public IState CommandReceived(CommandMessage commandMessage)
        {
            return this.OnCommandReceived(commandMessage);
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

            this.Logger.LogTrace($"Entering state {this.GetType().Name}.");

            this.OnEnter(message, machineData);
        }

        public void Exit()
        {
            if (this.hasExited)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already exited.");
            }

            this.hasExited = true;

            this.Logger.LogTrace($"Exiting state {this.GetType().Name}.");

            this.OnExit();
        }

        public virtual IState NotificationReceived(NotificationMessage notificationMessage)
        {
            return this.OnNotificationReceived(notificationMessage);
        }

        public IState Pause()
        {
            this.Logger.LogDebug($"Pausing state {this.GetType().Name}.");

            return this.OnPause();
        }

        public IState Resume()
        {
            this.Logger.LogDebug($"Resuming state {this.GetType().Name}.");
            return this.OnResume();
        }

        public IState Stop(StopRequestReason reason)
        {
            if (this.hasStopped)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already stopped.");
            }

            this.hasStopped = true;

            this.Logger.LogTrace($"Stopping state {this.GetType().Name}.");

            return this.OnStop(reason);
        }

        protected IState GetState<TState>()
            where TState : IState
        {
            return this.serviceProvider.GetRequiredService<TState>();
        }

        protected virtual IState OnAbort()
        {
            return this;
        }

        protected virtual IState OnCommandReceived(CommandMessage commandMessage)
        {
            return this;
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

        protected virtual IState OnPause()
        {
            return this;
        }

        protected virtual IState OnResume()
        {
            return this;
        }

        protected abstract IState OnStop(StopRequestReason reason);

        #endregion
    }
}
