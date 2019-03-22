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
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;

            var resetSecurityIoMessage = new IoMessage(false);
            resetSecurityIoMessage.SwitchResetSecurity(true);

            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.logger));
            }
        }

        #endregion
    }
}
