using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Horizontal switch is done
    public class HorizontalSwitchDoneState : IState
    {
        #region Fields

        private IWriteLogService data;

        private INewInverterDriver driver;

        private IEventAggregator eventAggregator;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HorizontalSwitchDoneState(StateMachineHoming parent, INewInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            //x this.data.LogWriting(new Command_EventParameter(CommandType.ExecuteHoming));

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);

            this.driver.ExecuteHorizontalHoming();
        }

        #endregion

        #region Properties

        public string Type => "Horizontal Switch Done";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            switch (notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        this.parent.ChangeState(new HorizontalHomingDoneState(this.parent, this.driver, this.data, this.eventAggregator));
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
