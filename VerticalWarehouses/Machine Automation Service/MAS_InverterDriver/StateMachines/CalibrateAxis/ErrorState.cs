using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine("ErrorState");
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            var messageData = new CalibrateAxisMessageData(this.axisToCalibrate);

            var errorNotification = new NotificationMessage(messageData, "Inverter operation error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.CalibrateAxis, MessageStatus.OperationError, ErrorLevel.Error);
            parentStateMachine.PublishNotificationEvent(errorNotification);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(InverterMessage message)
        {
            Console.WriteLine("ErrorState-ProcessMessage");
        }

        #endregion
    }
}
