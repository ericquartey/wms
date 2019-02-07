using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingIdleState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion Fields

        #region Constructors

        public HomingIdleState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));
        }

        #endregion Constructors

        #region Properties

        public string Type => "Homing Undone State";

        #endregion Properties
    }
}
