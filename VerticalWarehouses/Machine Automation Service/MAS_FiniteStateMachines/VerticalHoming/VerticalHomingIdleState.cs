using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class VerticalHomingIdleState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalHoming parent;

        #endregion

        #region Constructors

        public VerticalHomingIdleState( StateMachineVerticalHoming parent, INewInverterDriver driver, IEventAggregator eventAggregator )
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe( this.notifyEventHandler );

            this.driver.ExecuteVerticalHoming();
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Idle State";

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
                        this.parent.ChangeState( new VerticalHomingDoneState( this.parent, this.driver, this.eventAggregator ) );
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
