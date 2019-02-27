using System;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly MovingDrawer movingDrawer;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, MovingDrawer movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
