using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PulseResetState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        //public PulseResetState(IIoStateMachine parentStateMachine, ILogger logger)
        //{
        //    logger.LogDebug("1:Method Start");

        //    this.logger = logger;
        //    this.ParentStateMachine = parentStateMachine;

        //    var resetSecurityIoMessage = new IoMessage(false);

        //    this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

        //    resetSecurityIoMessage.SwitchResetSecurity(true);
        //    parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Constructors

        public PulseResetState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;

            var resetSecurityIoMessage = new IoSHDMessage(false);

            this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

            resetSecurityIoMessage.SwitchResetSecurity(true);
            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~PulseResetState()
        {
            this.Dispose(false);
        }

        #endregion

        //public override void ProcessMessage(IoMessage message)
        //{
        //    this.logger.LogDebug("1:Method Start");
        //    this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

        //    if (message.ValidOutputs && !message.ResetSecurity)
        //    {
        //        this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.logger));
        //    }

        //    this.logger.LogDebug("3:Method End");
        //}

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
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
