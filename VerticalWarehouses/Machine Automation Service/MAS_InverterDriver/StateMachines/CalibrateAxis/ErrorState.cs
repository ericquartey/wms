using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver
{
   public class ErrorState : IState
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly StateMachineCalibrateAxis stateMachineCalibrateAxis;

        #endregion

        #region Constructors

        public ErrorState(StateMachineCalibrateAxis stateMachineCalibrateAxis, IInverterDriver inverterDriver, IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineCalibrateAxis = stateMachineCalibrateAxis;

            this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Error State";

        #endregion

        #region Methods

        private void notifyEventHandler(Notification_EventParameter notification)
        {

            if (notification.OperationType == OperationType.SwitchHorizontalToVertical)
            {
                switch (notification.OperationStatus)
                {
                    case OperationStatus.End:
                        {
                            break;
                        }
                    case OperationStatus.Error:
                        {
                            var notifyEvent = new Notification_EventParameter(OperationType.Homing, OperationStatus.Error, "Unknown Operation!", Verbosity.Info);
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
