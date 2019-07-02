using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationEndState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationEndState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;
        }

        #endregion

        #region Destructors

        ~SetConfigurationEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug($"1: Received Message = {message.ToString()}");

            if (this.status.MatchOutputs(message.Outputs))
            {
                var endNotification = new FieldNotificationMessage(null, "Set configuration IO complete", FieldMessageActor.Any,
                    FieldMessageActor.IoDriver, FieldMessageType.SetConfigurationIO, MessageStatus.OperationEnd);

                this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var clearIoMessage = new IoSHDWriteMessage();
            clearIoMessage.Force = true;

            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.logger.LogTrace($"1:Clear IO={clearIoMessage}");

            this.ParentStateMachine.EnqueueMessage(clearIoMessage);
        }

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

        #endregion
    }
}
