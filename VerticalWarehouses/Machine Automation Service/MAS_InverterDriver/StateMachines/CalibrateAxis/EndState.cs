using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class EndState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - EndState:Ctor");
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

            var messageData = new CalibrateAxisMessageData(axisToCalibrate);
            var endNotification = new NotificationMessage(messageData, "Axis calibration complete", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.CalibrateAxis, MessageStatus.OperationEnd);
            this.parentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - EndState:ProcessMessage");
            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate));
            }

            return true;
        }

        #endregion
    }
}
