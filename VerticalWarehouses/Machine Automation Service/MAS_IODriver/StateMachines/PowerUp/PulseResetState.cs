using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PulseResetState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public PulseResetState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.parentStateMachine = parentStateMachine;

            var resetSecurityIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

            resetSecurityIoMessage.SwitchResetSecurity(true);
            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.logger));
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
