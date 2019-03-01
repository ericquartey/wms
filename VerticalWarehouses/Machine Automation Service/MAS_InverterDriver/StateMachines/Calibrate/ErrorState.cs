using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.InverterDriver.StateMachines.Calibrate
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, InverterMessage message)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            var notificationMessage = new NotificationMessage(null, "Inverter Driver Error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.Calibrate, MessageStatus.OperationError, ErrorLevel.Error);

            parentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
        }

        #endregion
    }
}
