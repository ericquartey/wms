using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    internal sealed class SetConfigurationEndState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public SetConfigurationEndState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoIndex index,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1: Received Message = {message.ToString()}");

            if (this.status.MatchOutputs(message.Outputs))
            {
                var endNotification = new FieldNotificationMessage(
                    null,
                    "Set configuration IO complete",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SetConfigurationIo,
                    MessageStatus.OperationEnd,
                    (byte)this.index);

                this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var clearIoMessage = new IoWriteMessage();

            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.Logger.LogDebug($"1:Clear IO={clearIoMessage}");

            this.ParentStateMachine.EnqueueMessage(clearIoMessage);
        }

        #endregion
    }
}
