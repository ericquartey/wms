using Ferretto.VW.MAS_FiniteStateMachines.Homing;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.INewInverterDriver driver;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineHoming(MAS_InverterDriver.INewInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.data = iWriteLogService;
            this.driver = iDriver;
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
            this.state = new HomingIdleState(this, this.driver, this.data);
        }

        #endregion
    }
}
