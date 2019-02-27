using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.InverterDriver.StateMachines.Calibrate
{
    public class EndState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            var notificationMessage = new NotificationMessage(null, "Inverter Driver Error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.Calibrate, MessageStatus.OperationEnd);

            parentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, message));
        }

        #endregion
    }
}
