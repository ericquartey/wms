using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class HorizontalMovingStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly MovingDrawer movingDrawer;

        private MovingDrawer currentMovingDrawer;

        #endregion

        #region Constructors

        public HorizontalMovingStateMachine(MovingDrawer movingDrawer,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            BlockingConcurrentQueue<InverterMessage> priorityInverterCommandQueue)
        {
            this.movingDrawer = movingDrawer;
            this.inverterCommandQueue = inverterCommandQueue;
            this.priorityInverterCommandQueue = priorityInverterCommandQueue;
        }

        #endregion

        #region Methods

        public override void ChangeState(IInverterState newState)
        {
            switch (this.movingDrawer)
            {
                case MovingDrawer.Horizontal:
                    this.currentMovingDrawer = MovingDrawer.Horizontal;
                    base.ChangeState(new IdleState(this, this.currentMovingDrawer));
                    break;
            }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.movingDrawer)
            {
                case MovingDrawer.Horizontal:
                    this.currentMovingDrawer = MovingDrawer.Horizontal;
                    break;
            }

            this.CurrentState = new IdleState(this, this.currentMovingDrawer);
        }

        #endregion
    }
}
