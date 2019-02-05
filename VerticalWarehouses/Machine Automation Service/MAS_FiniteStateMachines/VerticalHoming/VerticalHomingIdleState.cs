namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is undone
    public class VerticalHomingIdleState : IState
    {
        #region Fields

        private StateMachineVerticalHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public VerticalHomingIdleState(StateMachineVerticalHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Vertical homing state idle");

            // launch the command
            this.driver.ExecuteVerticalHoming();

            this.context.ChangeState(new VerticalHomingDoneState(this.context, this.driver, this.data));
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Idle State";

        #endregion
    }
}
