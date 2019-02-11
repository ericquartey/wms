using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The horizontal axis homing is done
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent, INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));

            this.parent.HorizontalHomingAlreadyDone = true;
        }

        #endregion

        #region Properties

        public string Type => "Horizontal homing done";

        #endregion
    }
}
