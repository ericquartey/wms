using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingIdleState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private StateMachineHoming parent;

        #endregion

        #region Constructors

        public HomingIdleState( StateMachineHoming parent, INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IEventAggregator eventAggregator )
        {
            this.parent = parent;
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Subscribe( this.notifyEventHandler );

            this.remoteIODriver.SwitchVerticalToHorizontal();
        }

        #endregion

        #region Properties

        public string Type => "Homing Idle State";

        #endregion

        #region Methods

        public void NotifyMessage( Event_Message message )
        {
            throw new System.NotImplementedException();
        }

        private void notifyEventHandler( Notification_EventParameter notification )
        {
            if(notification.OperationType == OperationType.SwitchVerticalToHorizontal)
            {
                switch(notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            this.parent.ChangeState( new HorizontalSwitchDoneState( this.parent, this.driver, this.remoteIODriver, this.data, this.eventAggregator ) );
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

            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Unsubscribe( this.notifyEventHandler );
        }

        #endregion
    }
}
