using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly IInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming context;

        #endregion Fields

        #region Constructors

        public VerticalHomingDoneState(StateMachineVerticalHoming parent, IInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Done State";

        #endregion Properties
    }
}
