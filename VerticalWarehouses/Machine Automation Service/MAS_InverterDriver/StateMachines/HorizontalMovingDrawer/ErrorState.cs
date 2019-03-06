using System;
using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;
        }

        #endregion

        #region Methods

        public override void ProcessMessage(InverterMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
