namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical axis homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public VerticalHomingDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Vertical homing done state");
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Done";

        #endregion
    }
}
