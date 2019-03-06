namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PulseResetState : IoStateBase
    {
        #region Constructors

        public PulseResetState(IIoStateMachine parentStateMachine)
        {
            this.parentStateMachine = parentStateMachine;
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
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine));
            }
        }

        #endregion
    }
}
