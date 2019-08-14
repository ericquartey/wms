using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerEnable
{
    public class PowerEnableStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private readonly bool enable;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
            bool enable,
            BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue,
            IoSHDStatus status,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(eventAggregator, logger)
        {
            this.enable = enable;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PowerEnableStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods


        public override void Start()
        {
            this.Logger.LogTrace($"1:Enable={this.enable}");

            this.Logger.LogTrace("2:Change State to PowerEnableStartState");
            this.CurrentState = new PowerEnableStartState(this.enable, this.status, this.Logger, this);

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Enable {this.enable} ",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.PowerEnable,
                MessageStatus.OperationStart);
            this.Logger.LogTrace($"3:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
            this.PublishNotificationEvent(notificationMessage);

            this.CurrentState?.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
