using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class VerticalPositioningIdleState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineVerticalPositioning parent;

        #endregion

        #region Constructors

        public VerticalPositioningIdleState(StateMachineVerticalPositioning parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Vertical Positioning Idle State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            //TODO Define the parameters for the call to driver
            //TODO Actually the parameter values are fixed
            var target = 2048;
            this.driver.ExecuteVerticalPosition(target, 0, 0, 0, 0, 0, true);
        }

        public void NotifyMessage(Event_Message message)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            this.driver.ExecuteVerticalPositionStop();

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new Notification_EventParameter(OperationType.Positioning, OperationStatus.Stopped, "Positioning stopped", Verbosity.Info);
            this.eventAggregator.GetEvent<FiniteStateMachines_NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(Notification_EventParameter notification)
        {
            switch (notification.OperationStatus)
            {
                case OperationStatus.End:
                    {
                        this.parent.ChangeState(new VerticalPositioningDoneState(this.parent, this.driver, this.eventAggregator));
                        break;
                    }
                case OperationStatus.Error:
                    {
                        this.parent.ChangeState(new VerticalPositioningErrorState(this.parent, this.driver, this.eventAggregator));
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
