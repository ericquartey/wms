namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class ClearOutputs : IoStateBase
    {
        #region Constructors

        public ClearOutputs(IIoStateMachine parentStateMachine)
        {
            this.parentStateMachine = parentStateMachine;
            var clearIoMessage = new IoMessage(false);
            clearIoMessage.Force = true;

            parentStateMachine.EnqueueMessage(clearIoMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ResetSecurity)
            {
                this.parentStateMachine.ChangeState(new PulseReset(this.parentStateMachine));
            }
        }

        #endregion
    }
}
