using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class VerticalHomingIdleState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly IInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming context;

        #endregion Fields

        #region Constructors

        public VerticalHomingIdleState(StateMachineVerticalHoming parent, IInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.NotifyEventHandler);

            this.driver.ExecuteVerticalHoming();
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Idle State";

        #endregion Properties

        #region Methods

        public void NotifyEventHandler(Notification_EventParameter notification)
        {
            switch (notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        this.context.ChangeState(new VerticalHomingDoneState(this.context, this.driver, this.data, this.eventAggregator));
                        break;
                    }
                case OperationStatus.Error:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion Methods
    }
}
