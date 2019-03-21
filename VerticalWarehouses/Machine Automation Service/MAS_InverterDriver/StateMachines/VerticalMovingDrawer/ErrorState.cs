using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer
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

        public override bool ProcessMessage(InverterMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
