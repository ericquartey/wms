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

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming parent;

        #endregion

        #region Constructors

        public VerticalHomingIdleState(StateMachineVerticalHoming parent, INewInverterDriver driver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);

            this.driver.ExecuteVerticalHoming();
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Idle State";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            switch (notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        this.parent.ChangeState(new VerticalHomingDoneState(this.parent, this.driver, this.data, this.eventAggregator));
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

        #endregion
    }
}
