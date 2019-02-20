using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class ErrorState : IState
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly StateMachineHorizontalMoving stateMachineHorizontalMoving;

        #endregion

        #region Constructors

        public ErrorState(StateMachineHorizontalMoving stateMachineHorizontalMoving, IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineHorizontalMoving = stateMachineHorizontalMoving;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Error State";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {

            if (notification.OperationType == OperationType.SwitchVerticalToHorizontal)
            {
                switch (notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            break;
                        }
                    case OperationStatus.Error:
                        {
                            var notifyEvent = new Notification_EventParameter(OperationType.SwitchVerticalToHorizontal, OperationStatus.Error, "Unknown Operation!", Verbosity.Info);
                            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Publish(notifyEvent);

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        #endregion
    }
}
