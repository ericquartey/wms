using System;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateMachineBase : IStateMachine
    {
        #region Fields

        protected readonly ILogger logger;

        protected BlockingConcurrentQueue<CommandMessage> stateMachineCommandQueue;

        private bool disposed;

        #endregion

        #region Constructors

        protected StateMachineBase(IEventAggregator eventAggregator, ILogger logger)
        {
            this.logger = logger;
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

        public bool OperationDone { get; set; }

        protected IState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">The new state</param>
        /// <param name="message">A message to be published</param>
        public void ChangeState(IState newState, CommandMessage message = null)
        {
            this.CurrentState = newState;
            this.logger.LogTrace($"1:{newState.GetType()}");
            if (message != null)
            {
                this.logger.LogTrace($"2:{newState.GetType()}{message.Type}:{message.Destination}");
                this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Process the command message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be parsed.</param>
        public abstract void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process the notification message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be parsed.</param>
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Publish a given Command message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be sent.</param>
        public virtual void PublishCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"2:{message.Type}:{message.Destination}");
            this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        /// <summary>
        /// Publish a given Notification message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be sent.</param>
        public virtual void PublishNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"3:{message.Type}:{message.Destination}");
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        /// <summary>
        /// Start the states machine.
        /// </summary>
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
