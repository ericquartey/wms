using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineVerticalHoming : IState, IStateMachine
    {
        #region Fields

        private MAS_InverterDriver.InverterDriver driver;

        private IState state;

        #endregion Fields

        #region Constructors

        public StateMachineVerticalHoming()
        {
            this.driver = Singleton<MAS_InverterDriver.InverterDriver>.UniqueInstance;
        }

        #endregion Constructors

        #region Properties

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
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        //TODO await driver.ExecuteAction("Horizontal Home");
                        break;
                    }
                case IdOperation.SwitchHorizontalToVertical:
                    {
                        //TODO await driver.ExecuteAction("SwitchHorizontalToVertical");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        //TODO await driver.ExecuteAction("Vertical Home");
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        //TODO await driver.ExecuteAction("SwitchVerticalToHorizontal");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void Start()
        {
            //TODO check the sensors before to set the initial state
            this.state = new VerticalHomingUndoneState(this);
        }

        #endregion Methods
    }
}
