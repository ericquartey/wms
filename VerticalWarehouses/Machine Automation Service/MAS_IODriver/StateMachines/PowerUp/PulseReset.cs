using System;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PulseReset : IoStateBase
    {
        #region Constructors

        public PulseReset(IIoStateMachine parentStateMachine)
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
