using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PulseResetState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private bool ackResetSecurityON;

        private bool disposed;

        private IoSHDStatus status;

        #endregion

        #region Constructors

        public PulseResetState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;
            this.ackResetSecurityON = false;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~PulseResetState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            // Acknowledge the reset security ON message has been processed
            if (this.status.MatchOutputs(message.Outputs) && !this.ackResetSecurityON)
            {
                this.ackResetSecurityON = true;
            }

            var checkMessage = (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                                message.ValidOutputs &&
                                !message.ResetSecurity);

            if (this.ackResetSecurityON && checkMessage)
            {
                // Change state
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            var resetSecurityIoMessage = new IoSHDWriteMessage();

            resetSecurityIoMessage.SwitchResetSecurity(true);
            this.logger.LogTrace($"2:Switch Security IO={resetSecurityIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
            }

            this.ackResetSecurityON = false;
            this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);

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
