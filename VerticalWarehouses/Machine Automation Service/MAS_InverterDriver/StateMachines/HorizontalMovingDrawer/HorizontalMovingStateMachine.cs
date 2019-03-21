using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class HorizontalMovingStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        private Axis currentMovingDrawer;

        #endregion

        #region Constructors

        public HorizontalMovingStateMachine(Axis movingDrawer,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            BlockingConcurrentQueue<InverterMessage> priorityInverterCommandQueue)
        {
            this.movingDrawer = movingDrawer;
            this.inverterCommandQueue = inverterCommandQueue;
            //this.priorityInverterCommandQueue = priorityInverterCommandQueue;
        }

        #endregion

        #region Methods

        public override void ChangeState(IInverterState newState)
        {
            switch (this.movingDrawer)
            {
                case Axis.Horizontal:
                    this.currentMovingDrawer = Axis.Horizontal;
                    base.ChangeState(new IdleState(this, this.currentMovingDrawer));
                    break;
            }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.movingDrawer)
            {
                case Axis.Horizontal:
                    this.currentMovingDrawer = Axis.Horizontal;
                    break;
            }

            this.CurrentState = new IdleState(this, this.currentMovingDrawer);
        }

        #endregion
    }
}
