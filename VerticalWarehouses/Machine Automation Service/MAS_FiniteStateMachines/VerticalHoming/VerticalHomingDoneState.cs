using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming parent;

        #endregion

        #region Constructors

        public VerticalHomingDoneState(StateMachineVerticalHoming parent, INewInverterDriver driver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Homing done", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Done State";

        #endregion
    }
}
