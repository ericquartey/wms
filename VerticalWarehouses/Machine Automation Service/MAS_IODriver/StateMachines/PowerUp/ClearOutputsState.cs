namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class ClearOutputsState : IoStateBase
    {
        #region Constructors

        public ClearOutputsState(IIoStateMachine parentStateMachine)
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
            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.parentStateMachine.ChangeState(new PulseResetState(this.parentStateMachine));
            }
        }

        #endregion
    }
}
