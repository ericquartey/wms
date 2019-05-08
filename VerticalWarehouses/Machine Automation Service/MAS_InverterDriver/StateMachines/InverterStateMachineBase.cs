using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        protected IEventAggregator EventAggregator;

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        protected ILogger Logger;

        private const int CONTROL_WORD_TIMEOUT = 5000;

        private readonly Timer controlWordCheckTimer;

        private bool disposed;

        #endregion

        #region Constructors

        protected InverterStateMachineBase(ILogger logger)
        {
            this.Logger = logger;
            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);
        }

        #endregion

        #region Destructors

        ~InverterStateMachineBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected IInverterState CurrentState { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void ChangeState(IInverterState newState)
        {
            this.Logger.LogDebug("1:Method Start");

            this.Logger.LogTrace($"2:new State: {newState.GetType()}");

            this.CurrentState = newState;
            this.CurrentState.Start();

            this.Logger.LogDebug("3:Method End");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void EnqueueMessage(InverterMessage message)
        {
            this.controlWordCheckTimer.Change(CONTROL_WORD_TIMEOUT, Timeout.Infinite);
            this.InverterCommandQueue.Enqueue(message);
        }

        /// <inheritdoc />
        public virtual void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
        }

        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public bool ValidateCommandMessage(InverterMessage message)
        {
            bool returnValue = this.CurrentState?.ValidateCommandMessage(message) ?? false;
            if (returnValue)
            {
                this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            }
            return returnValue;
        }

        /// <inheritdoc />
        public bool ValidateCommandResponse(InverterMessage message)
        {
            return this.CurrentState?.ValidateCommandResponse(message) ?? false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.controlWordCheckTimer?.Dispose();
            }

            this.disposed = true;
        }

        private void ControlWordCheckTimeout(object state)
        {
            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            var errorNotification = new FieldNotificationMessage(null,
                "Control Word set timeout",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterOperationTimeout,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.PublishNotificationEvent(errorNotification);
            //TODO move current FSM to relevant EndState (Backlog item 2646)
        }

        #endregion
    }
}
