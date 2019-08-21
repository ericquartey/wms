using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationEndState : IoStateBase
    {

        #region Fields

        private readonly IoStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationEndState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SetConfigurationEndState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogDebug($"1: Received Message = {message.ToString()}");

            if (this.status.MatchOutputs(message.Outputs))
            {
                var endNotification = new FieldNotificationMessage(
                    null,
                    "Set configuration IO complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SetConfigurationIo,
                    MessageStatus.OperationEnd);

                this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var clearIoMessage = new IoWriteMessage();
            clearIoMessage.Force = true;

            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.Logger.LogTrace($"1:Clear IO={clearIoMessage}");

            this.ParentStateMachine.EnqueueMessage(clearIoMessage);
        }

        #endregion
    }
}
