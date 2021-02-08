using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerEnable
{
    internal sealed class PowerEnableStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly IoIndex deviceIndex;

        private readonly bool enable;

        private readonly IoStatus status;

        private bool isDisposed;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
            bool enable,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoIndex deviceIndex,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, ioCommandQueue, serviceScopeFactory)
        {
            this.enable = enable;
            this.status = status;
            this.deviceIndex = deviceIndex;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogTrace($"1:Enable={this.enable}");

            this.Logger.LogTrace("2:Change State to PowerEnableStartState");

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Enable {this.enable} ",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.PowerEnable,
                MessageStatus.OperationStart,
                (byte)this.deviceIndex);
            this.Logger.LogTrace($"3:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
            this.PublishNotificationEvent(notificationMessage);

            this.ChangeState(new PowerEnableStartState(this.enable, this.status, this.Logger, this, this.deviceIndex));
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
