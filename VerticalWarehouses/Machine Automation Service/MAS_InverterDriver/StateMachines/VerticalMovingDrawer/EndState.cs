using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer
{
    public class EndState : InverterStateBase
    {
        #region Fields

        private readonly MovingDrawer movingDrawer;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, MovingDrawer movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.movingDrawer));
        }

        #endregion
    }
}
