using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.BayLight
{
    internal sealed class BayLightStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly bool enable;

        private readonly IoIndex index;

        private readonly IoStatus status;

        private bool isDisposed;

        #endregion

        #region Constructors

        public BayLightStateMachine(
            bool enable,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoIndex index,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory
            )
            : base(eventAggregator, logger, ioCommandQueue, serviceScopeFactory)
        {
            this.enable = enable;
            this.index = index;
            this.status = status;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogTrace($"1:Enable BayLight={this.enable}");

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Enable Bay light = {this.enable} ",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.BayLight,
                MessageStatus.OperationStart,
                (byte)this.index);
            this.Logger.LogTrace($"2:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
            this.PublishNotificationEvent(notificationMessage);

            var state = new BayLightStartState(this.enable, this.status, this.Logger, this, this.index);
            this.ChangeState(state);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.CurrentState is System.IDisposable disposableState)
                {
                    disposableState.Dispose();
                }
            }

            this.isDisposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
