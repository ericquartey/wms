using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public VerticalSwitchDoneState( StateMachineHoming parent, INewInverterDriver iDriver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator )
        {
            this.parent = parent;
            this.driver = iDriver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe( this.notifyEventHandler );

            this.driver.ExecuteVerticalHoming();
        }

        #endregion

        #region Properties

        public string Type => "Vertical Switch Done";

        #endregion

        #region Methods

        public void NotifyMessage( Event_Message message )
        {
            throw new System.NotImplementedException();
        }

        private void notifyEventHandler( Notification_EventParameter notification )
        {
            switch(notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        if(notification.Description == "Vertical Calibration Ended")
                        {
                            this.parent.ChangeState( new VerticalHomingDoneState( this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator ) );
                        }
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
