using System;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class EndState : IoStateBase
    {
        #region Constructors

        public EndState(IIoStateMachine parentStateMachine)
        {
            this.parentStateMachine = parentStateMachine;
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
