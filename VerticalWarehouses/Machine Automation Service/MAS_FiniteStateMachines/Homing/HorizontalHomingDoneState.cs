namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The horizontal axis homing is done
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Homing done state");

            this.context.HorizontalHomingAlreadyDone = true;
        }

        #endregion

        #region Properties

        public string Type => "Horizontal homing done";

        #endregion
    }
}
