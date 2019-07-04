using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetOutputsState : IoStateBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetOutputsState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;

            logger.LogTrace("1:Method Start");
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
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs && message.OutputsCleared;

            if (this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.Logger));
            }
        }

        public override void Start()
        {
            var resetIoMessage = new IoSHDWriteMessage();
            resetIoMessage.Force = true;

            this.Logger.LogTrace($"1:Reset IO={resetIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetIoMessage.Outputs);
            }
            this.ParentStateMachine.EnqueueMessage(resetIoMessage);
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
