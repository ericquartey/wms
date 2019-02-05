using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineVerticalHoming : IState, IStateMachine
    {
        #region Fields

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineVerticalHoming(MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;
        }

        #endregion

        #region Properties

        public string Type => this.state.Type;

        #endregion

        #region Methods

        public void ChangeState(IState newState)
        {
            this.state = newState;
        }

        /*
        public void DoAction(IdOperation code)
        {
            this.state.DoAction(code);
        }
        */

        public void ExecuteOperation(IdOperation code)
        {
            //this.fsm.MakeOperationByInverter(code);
        }

        public void Start()
        {
            // TODO check the sensors before to set the initial state
            this.state = new VerticalHomingIdleState(this, this.driver, this.data);
        }

        #endregion
    }
}
