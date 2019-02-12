using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical switch is done
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public VerticalSwitchDoneState(StateMachineHoming parent, INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            //x this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);

            this.driver.ExecuteVerticalHoming();
        }

        #endregion

        #region Properties

        public string Type => "Vertical Switch Done";

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
