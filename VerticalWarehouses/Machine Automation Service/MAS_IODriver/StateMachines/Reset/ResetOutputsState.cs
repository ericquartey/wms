using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetOutputsState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~ResetOutputsState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs && message.OutputsCleared;

            if (this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            var resetIoMessage = new IoSHDWriteMessage();
            resetIoMessage.Force = true;

            this.logger.LogTrace($"2:Reset IO={resetIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(resetIoMessage);

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
