using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationStartState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationStartState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;
        }

        #endregion

        #region Destructors

        ~SetConfigurationStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Ack)
            {
                this.logger.LogTrace($"2:Format data operation message={message.FormatDataOperation}");

                this.ParentStateMachine.ChangeState(new SetConfigurationEndState(this.ParentStateMachine, this.status, this.logger));
            }
        }

        public override void Start()
        {
            var message = new IoSHDWriteMessage(
                this.status.ComunicationTimeOut,
                this.status.UseSetupOutputLines,
                this.status.SetupOutputLines,
                this.status.DebounceInput);

            this.logger.LogDebug($"1: ConfigurationMessage [comTout={this.status.ComunicationTimeOut}]");

            this.ParentStateMachine.EnqueueMessage(message);
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
