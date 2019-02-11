using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineHoming(INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool HorizontalHomingAlreadyDone { get; set; }

        public string Type => this.state.Type;

        #endregion

        #region Methods

        public void ChangeState(IState newState)
        {
            this.state = newState;
        }

        public void Start()
        {
            this.HorizontalHomingAlreadyDone = false;

            //TODO check the sensors before to set the initial state
            this.state = new HomingIdleState(this, this.driver, this.data, this.eventAggregator);
        }

        #endregion
    }
}
