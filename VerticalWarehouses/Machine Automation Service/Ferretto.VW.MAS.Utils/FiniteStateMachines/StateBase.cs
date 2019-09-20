using System;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.Utils
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
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.Logger = logger;
        }

        #endregion



        #region Properties

        protected ILogger<StateBase> Logger { get; }

        #endregion



        #region Methods

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.OnDisposing();
                this.isDisposed = true;
            }
        }

        public void Enter(IMessageData data)
        {
            if (this.hasEntered)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already entered.");
            }

            this.hasEntered = true;

            this.Logger.LogDebug($"Entering state {this.GetType()}.");

            this.OnEnter(data);
        }

        public void Exit()
        {
            if (!this.hasExited)
            {
                throw new InvalidOperationException($"FSM State {this.GetType().Name} was already exited.");
            }

            this.hasExited = true;

            this.Logger.LogDebug($"Exiting state {this.GetType()}.");

            this.OnExit();
        }

        protected virtual void OnDisposing()
        {
            // do nothing
            // derived classes can customize the behaviour of this method
        }

        protected virtual void OnEnter(IMessageData data)
        {
            // do nothing
        }

        protected virtual void OnExit()
        {
            // do nothing
        }

        #endregion
    }
}
