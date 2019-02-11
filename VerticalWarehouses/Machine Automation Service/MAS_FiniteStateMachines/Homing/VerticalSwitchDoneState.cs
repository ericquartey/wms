using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical switch is done
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.INewInverterDriver driver;

        #endregion Fields

        #region Constructors

        public VerticalSwitchDoneState(StateMachineHoming parent, MAS_InverterDriver.INewInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Switch Done";

        #endregion Properties
    }
}
