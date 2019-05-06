using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class ClearOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        //public ClearOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        //{
        //    logger.LogDebug("1:Method Start");

        //    this.logger = logger;
        //    this.ParentStateMachine = parentStateMachine;
        //    var clearIoMessage = new IoMessage(false);
        //    clearIoMessage.Force = true;

        //    this.logger.LogTrace($"2:Clear IO={clearIoMessage}");

        //    parentStateMachine.EnqueueMessage(clearIoMessage);

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Constructors

        public ClearOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            var clearIoMessage = new IoSHDMessage(false);
            clearIoMessage.Force = true;

            this.logger.LogTrace($"2:Clear IO={clearIoMessage}");

            parentStateMachine.EnqueueMessage(clearIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~ClearOutputsState()
        {
            this.Dispose(false);
        }

        #endregion

        //public override void ProcessMessage(IoMessage message)
        //{
        //    this.logger.LogDebug("1:Method Start");

        //    this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

        //    if (message.ValidOutputs && message.OutputsCleared)
        //    {
        //        this.ParentStateMachine.ChangeState(new PulseResetState(this.ParentStateMachine, this.logger));
        //    }

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState(new PulseResetState(this.ParentStateMachine, this.logger));
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
