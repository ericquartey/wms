﻿using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines
{
    internal abstract class IoStateMachineBase : IIoStateMachine
    {
        #region Fields

        private bool isDisposed;

        #endregion

        #region Constructors

        public IoStateMachineBase(
            IEventAggregator eventAggregator,
            ILogger logger,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue)
        {
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.IoCommandQueue = ioCommandQueue ?? throw new ArgumentNullException(nameof(ioCommandQueue));
        }

        #endregion

        #region Properties

        protected IIoState CurrentState { get; private set; }

        protected IEventAggregator EventAggregator { get; }

        protected BlockingConcurrentQueue<IoWriteMessage> IoCommandQueue { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public void ChangeState(IIoState newState)
        {
            if (newState is null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.IoDriver, newState.GetType().Name, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"IoDriver current state {newState.GetType().Name}",
                MessageActor.Any,
                MessageActor.IoDriver,
                MessageType.MachineStateActive,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationStart);

            this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            if (this.CurrentState is IDisposable disposableState)
            {
                disposableState.Dispose();
            }

            this.CurrentState = newState;
            this.CurrentState.Start();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void EnqueueMessage(IoWriteMessage message)
        {
            this.IoCommandQueue.Enqueue(message);
        }

        public virtual void ProcessMessage(IoMessage message)
        {
            this.CurrentState?.ProcessMessage(message);
        }

        public virtual void ProcessResponseMessage(IoReadMessage message)
        {
            this.CurrentState?.ProcessResponseMessage(message);
        }

        public void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.EventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
        }

        public abstract void Start();

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            // This is wrong: this code must not be here.
            // Do not make improper use of disposal methods, please.
            var notificationMessageData = new MachineStateActiveMessageData(MessageActor.IoDriver, string.Empty, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"IoDriver current state null",
                MessageActor.Any,
                MessageActor.IoDriver,
                MessageType.MachineStateActive,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationStart);

            this.EventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            this.isDisposed = true;
        }

        #endregion
    }
}
