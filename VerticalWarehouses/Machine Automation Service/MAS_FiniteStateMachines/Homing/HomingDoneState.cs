namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Complete homing is done
    public class HomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public HomingDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Homing state done");
        }

        #endregion

        #region Properties

        public string Type => "Homing Done State";

        #endregion
    }
}
