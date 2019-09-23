// ReSharper disable ArrangeThisQualifier
using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public abstract class StateBase : IState, IDisposable
    {

        #region Fields

        private bool hasEntered;

        private bool hasExited;

        private bool isDisposed;

        #endregion

        #region Constructors

        protected StateBase(ILogger<StateBase> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if(!this.isDisposed)
            {
                this.OnDisposing();
                this.isDisposed = true;
            }
        }

        public void Enter(CommandMessage message)
        {
            if(this.hasEntered)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already entered.");
            }

            this.hasEntered = true;

            this.Logger.LogDebug($"Entering state {this.GetType()}.");

            this.OnEnter(message);
        }

        public void Exit()
        {
            if(this.hasExited)
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

        protected virtual IState OnCommandReceived(CommandMessage commandMessage)
        {
            return this;
        }

        protected virtual void OnDisposing()
        {
            // do nothing
            // derived classes can customize the behaviour of this method
        }

        protected virtual void OnEnter(CommandMessage commandMessage)
        {
            // do nothing
        }

        protected virtual void OnExit()
        {
            // do nothing
        }

        protected virtual IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            return this;
        }

        #endregion
    }
}
