using Ferretto.VW.MAS_FiniteStateMachines.Homing;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        private FiniteStateMachines fsm;

        private IState state;

        #endregion Fields

        #region Constructors

        public StateMachineHoming(FiniteStateMachines fsm)
        {
            this.fsm = fsm;
        }

        #endregion Constructors

        #region Properties

        public bool HorizontalHomingAlreadyDone { get; set; }

        public string Type => this.state.Type;

        #endregion Properties

        #region Methods

        public void ChangeState(IState newState)
        {
            this.state = newState;
        }

        public void DoAction(IdOperation code)
        {
            this.state.DoAction(code);
        }

        public void ExecuteOperation(IdOperation code)
        {
            this.fsm.MakeOperationByInverter(code);
        }

        public void Start()
        {
            this.HorizontalHomingAlreadyDone = false;
            //TODO check the sensors before to set the initial state
            this.state = new HomingUndoneState(this);
        }

        #endregion Methods
    }
}
