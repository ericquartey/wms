using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class ErrorState : IState
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterDriver inverterDriver;

        private readonly StateMachineCalibrateAxis stateMachineCalibrateAxis;

        #endregion

        #region Constructors

        public ErrorState(StateMachineCalibrateAxis stateMachineCalibrateAxis, IInverterDriver inverterDriver,
            IEventAggregator eventAggregator)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.stateMachineCalibrateAxis = stateMachineCalibrateAxis;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public string Type => "Error State";

        #endregion

        #region Methods

        private void notifyEventHandler(NotificationMessage notification)
        {
            if (notification.Type == MessageType.Homing)
                switch (notification.Status)
                {
                    case MessageStatus.OperationEnd:
                        {
                            break;
                        }
                    case MessageStatus.OperationError:
                        {
                            var notifyEvent = new NotificationMessage(null, "Unknown Operation!", MessageActor.Any,
                                MessageActor.InverterDriver, MessageType.Homing, MessageStatus.OperationError,
                                ErrorLevel.Error);
                            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);

                            break;
                        }
                }
        }

        #endregion
    }
}
