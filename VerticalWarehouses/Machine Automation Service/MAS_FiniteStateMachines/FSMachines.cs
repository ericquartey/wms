// using Ferretto.VW.Common_Utils.Patterns;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FSMachines : IFSMachines
    {
        #region Fields

        //private InverterDriver driver;
        private StateMachineHoming homing;

        private StateMachineVerticalHoming verticalHoming;

        #endregion Fields

        #region Constructors

        public FSMachines()
        {
            // create the state machine for the homing
            this.homing = new StateMachineHoming();
            // create the state machine for the vertical homing
            this.verticalHoming = new StateMachineVerticalHoming();
            //this.driver = Singleton<InverterDriver>.UniqueInstance;
        }

        #endregion Constructors

        #region Methods

        public void DoHoming()
        {
            this.homing?.Start();
        }

        public void DoVerticalHoming()
        {
            this.verticalHoming?.Start();
        }

        #endregion Methods
    }
}
