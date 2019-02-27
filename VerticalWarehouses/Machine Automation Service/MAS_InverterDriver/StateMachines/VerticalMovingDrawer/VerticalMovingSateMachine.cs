using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class VerticalMovingStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly MovingDrawer movingDrawer;

        private MovingDrawer currentMovingDrawer;

        #endregion

        #region Constructors

        public VerticalMovingStateMachine(MovingDrawer movingDrawer,
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
                case MovingDrawer.Vertical:
                    this.currentMovingDrawer = MovingDrawer.Vertical;
                    base.ChangeState(new IdleState(this, this.currentMovingDrawer));
                    break;
            }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.movingDrawer)
            {
                case MovingDrawer.Vertical:
                    this.currentMovingDrawer = MovingDrawer.Vertical;
                    break;
            }

            this.CurrentState = new IdleState(this, this.currentMovingDrawer);
        }

        #endregion
    }
}
