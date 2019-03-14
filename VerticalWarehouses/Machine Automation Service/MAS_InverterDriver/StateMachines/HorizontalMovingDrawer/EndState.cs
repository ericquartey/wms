using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class EndState : InverterStateBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, Axis movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.movingDrawer));

            return false;
        }

        #endregion
    }
}
