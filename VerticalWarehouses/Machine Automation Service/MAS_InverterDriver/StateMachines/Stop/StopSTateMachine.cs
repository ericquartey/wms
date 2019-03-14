using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Prism.Events;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class StopStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private bool disposed;

        #endregion

        #region Constructors

        public StopStateMachine(Axis axisToStop, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator)
        {
            this.axisToStop = axisToStop;
            this.inverterCommandQueue = inverterCommandQueue;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~StopStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            CurrentState = new StopState(this, this.axisToStop);
        }

        #endregion
    }
}
