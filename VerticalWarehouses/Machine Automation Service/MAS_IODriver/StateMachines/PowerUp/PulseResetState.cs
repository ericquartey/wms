using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    public class PulseResetState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoSHDStatus status;

        private bool ackResetSecurityON;

        private bool disposed;

        #endregion

        #region Constructors

        public PulseResetState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
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

        ~PulseResetState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            //TEMP Acknowledge the reset security ON message has been processed
            if (this.status.MatchOutputs(message.Outputs) && !this.ackResetSecurityON)
            {
                this.ackResetSecurityON = true;
            }

            var checkMessage =
                message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data
                &&
                message.ValidOutputs
                &&
                !message.ResetSecurity;

            if (this.ackResetSecurityON && checkMessage)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var resetSecurityIoMessage = new IoSHDWriteMessage();

            resetSecurityIoMessage.SwitchResetSecurity(true);
            this.Logger.LogTrace($"1:Switch Security IO={resetSecurityIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
            }

            this.ackResetSecurityON = false;
            this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);
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
