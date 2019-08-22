﻿using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationStartState : IoStateBase
    {

        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationStartState(
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

        #region Destructors

        ~SetConfigurationStartState()
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
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Ack)
            {
                this.Logger.LogTrace($"2:Format data operation message={message.FormatDataOperation}");

                this.ParentStateMachine.ChangeState(new SetConfigurationEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var message = new IoWriteMessage(
                this.status.ComunicationTimeOut,
                this.status.UseSetupOutputLines,
                this.status.SetupOutputLines,
                this.status.DebounceInput);

            this.Logger.LogDebug($"1: ConfigurationMessage [comTout={this.status.ComunicationTimeOut}]");

            this.ParentStateMachine.EnqueueMessage(message);
        }

        #endregion
    }
}
