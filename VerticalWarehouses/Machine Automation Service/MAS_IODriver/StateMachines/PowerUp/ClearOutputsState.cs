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

        #region Constructors

        public ClearOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.parentStateMachine = parentStateMachine;
            var clearIoMessage = new IoMessage(false);
            clearIoMessage.Force = true;

            this.logger.LogTrace($"2:{clearIoMessage}");

            parentStateMachine.EnqueueMessage(clearIoMessage);

            this.logger.LogDebug("3:Method Start");
        }

        #endregion

        #region Destructors

        ~ClearOutputsState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:{message.ValidOutputs}:{message.OutputsCleared}");

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.parentStateMachine.ChangeState(new PulseResetState(this.parentStateMachine, this.logger));
            }

            this.logger.LogDebug("3:Method Start");
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
