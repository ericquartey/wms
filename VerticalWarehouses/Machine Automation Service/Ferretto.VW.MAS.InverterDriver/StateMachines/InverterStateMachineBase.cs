using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        private const int CONTROL_WORD_TIMEOUT = 5000;

        //private readonly Timer controlWordCheckTimer;
        private bool disposed;

        #endregion

        #region Constructors

        protected InverterStateMachineBase(
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.Logger = logger;
            this.EventAggregator = eventAggregator;
            this.InverterCommandQueue = inverterCommandQueue;

            //this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);

            this.Logger.LogTrace($"Inverter FSM '{this.GetType().Name}' initialized.");
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

        protected IEventAggregator EventAggregator { get; }

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void ChangeState(IInverterState newState)
        {
            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.InverterDriver, newState.GetType().Name, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Inverter current state {newState.GetType().Name}",
                MessageActor.Any,
                MessageActor.InverterDriver,
                MessageType.MachineStateActive,
                MessageStatus.OperationStart);

            this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            this.Logger.LogTrace($"1:new State: {newState.GetType()}");

            this.CurrentState = newState;
            this.CurrentState.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void EnqueueMessage(InverterMessage message)
        {
            //this.controlWordCheckTimer.Change(CONTROL_WORD_TIMEOUT, Timeout.Infinite);
            this.Logger.LogTrace($"1:Enqueue message {message}");
            this.InverterCommandQueue.Enqueue(message);
        }

        /// <inheritdoc />
        public virtual void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
        }

        /// <inheritdoc />
        public void Release()
        {
            this.CurrentState?.Release();
        }

        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public abstract void Stop();

        /// <inheritdoc />
        public bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = this.CurrentState?.ValidateCommandMessage(message) ?? false;
            if (returnValue)
            {
                //this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
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
            this.Logger.LogDebug($"Disposing {this.GetType()}");

            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                //this.controlWordCheckTimer?.Dispose();
            }

            {
                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.InverterDriver, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"Inverter current status null",
                    MessageActor.Any,
                    MessageActor.InverterDriver,
                    MessageType.MachineStatusActive,
                    MessageStatus.OperationStart);

                this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }

            {
                var notificationMessageData = new MachineStateActiveMessageData(MessageActor.InverterDriver, string.Empty, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    $"Inverter current state null",
                    MessageActor.Any,
                    MessageActor.InverterDriver,
                    MessageType.MachineStateActive,
                    MessageStatus.OperationStart);

                this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }

            this.disposed = true;
        }

        private void ControlWordCheckTimeout(object state)
        {
            //this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            var errorNotification = new FieldNotificationMessage(
                null,
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
