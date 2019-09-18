﻿using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines
{
    internal abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isDisposed;

        #endregion

        #region Constructors

        protected InverterStateMachineBase(
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            this.Logger = logger;
            this.EventAggregator = eventAggregator;
            this.InverterCommandQueue = inverterCommandQueue;
            this.serviceScopeFactory = serviceScopeFactory;

            this.Logger.LogTrace($"Inverter '{this.GetType().Name}' FSM initialized.");
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
        public void EnqueueCommandMessage(InverterMessage message)
        {
            if (this.InverterCommandQueue.Count(x => x.ParameterId == message.ParameterId && x.SystemIndex == message.SystemIndex) < 2)
            {
                this.Logger.LogTrace($"1:Enqueue message {message}");
                this.InverterCommandQueue.Enqueue(message);
            }
        }

        public TService GetRequiredService<TService>()
            where TService : class
        {
            return this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<TService>();
        }

        /// <inheritdoc />
        public virtual void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.Logger.LogTrace($"2:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

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
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.CurrentState is IDisposable disposableState)
                {
                    disposableState.Dispose();
                }
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

            this.isDisposed = true;
        }

        #endregion
    }
}
