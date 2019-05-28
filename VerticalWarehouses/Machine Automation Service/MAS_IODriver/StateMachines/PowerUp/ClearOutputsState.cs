using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class ClearOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public ClearOutputsState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~ClearOutputsState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

            if (message.CodeOperation == Enumerations.SHDCodeOperation.Data &&
                message.ValidOutputs &&
                message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new PulseResetState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs &&
                message.OutputsCleared;

            //TEMP Check the matching between the status output flags and the message output flags (i.e. the clear output message has been processed)
            if (this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new PulseResetState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            var clearIoMessage = new IoSHDWriteMessage();
            clearIoMessage.Force = true;

            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.logger.LogTrace($"2:Clear IO={clearIoMessage}");

            this.ParentStateMachine.EnqueueMessage(clearIoMessage);

            this.logger.LogDebug("3:Method End");
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
