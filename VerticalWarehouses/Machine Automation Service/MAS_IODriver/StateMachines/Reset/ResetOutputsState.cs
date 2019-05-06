using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        //public ResetOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        //{
        //    logger.LogDebug("1:Method Start");

        //    this.logger = logger;
        //    this.ParentStateMachine = parentStateMachine;
        //    var resetIoMessage = new IoMessage(false);
        //    resetIoMessage.Force = true;

        //    this.logger.LogTrace($"2:Reset IO={resetIoMessage}");

        //    parentStateMachine.EnqueueMessage(resetIoMessage);

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Constructors

        public ResetOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            var resetIoMessage = new IoSHDMessage(false);
            resetIoMessage.Force = true;

            this.logger.LogTrace($"2:Reset IO={resetIoMessage}");

            parentStateMachine.EnqueueMessage(resetIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~ResetOutputsState()
        {
            this.Dispose(false);
        }

        #endregion

        //public override void ProcessMessage(IoMessage message)
        //{
        //    this.logger.LogDebug("1:Method Start");

        //    this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

        //    if (message.ValidOutputs && message.OutputsCleared)
        //    {
        //        this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.logger));
        //    }

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.logger));
            }

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
