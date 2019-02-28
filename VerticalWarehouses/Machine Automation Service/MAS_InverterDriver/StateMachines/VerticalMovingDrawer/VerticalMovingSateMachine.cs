using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class VerticalMovingStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        private Axis currentMovingDrawer;

        #endregion

        #region Constructors

        public VerticalMovingStateMachine(Axis movingDrawer,
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
                case Axis.Vertical:
                    this.currentMovingDrawer = Axis.Vertical;
                    base.ChangeState(new IdleState(this, this.currentMovingDrawer));
                    break;
            }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.movingDrawer)
            {
                case Axis.Vertical:
                    this.currentMovingDrawer = Axis.Vertical;
                    break;
            }

            this.CurrentState = new IdleState(this, this.currentMovingDrawer);
        }

        #endregion
    }
}
