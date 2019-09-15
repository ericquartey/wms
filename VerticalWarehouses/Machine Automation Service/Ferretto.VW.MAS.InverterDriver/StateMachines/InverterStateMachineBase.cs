using System;
using System.Linq;
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
    internal abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

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

            this.Logger.LogTrace($"Inverter FSM '{this.GetType().Name}' initialized.");
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
        }

        /// <inheritdoc />
        public void EnqueueMessage(InverterMessage message)
        {
            if (this.InverterCommandQueue.Count(x => x.ParameterId == message.ParameterId && x.SystemIndex == message.SystemIndex) < 2)
            {
                this.Logger.LogTrace($"1:Enqueue message {message}");
                this.InverterCommandQueue.Enqueue(message);
            }
        }

        /// <inheritdoc />
        public virtual void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
        }

        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public abstract void Stop();

        /// <inheritdoc />
        public bool ValidateCommandMessage(InverterMessage message)
        {
            return this.CurrentState?.ValidateCommandMessage(message) ?? false;
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

            this.disposed = true;
        }

        #endregion
    }
}
