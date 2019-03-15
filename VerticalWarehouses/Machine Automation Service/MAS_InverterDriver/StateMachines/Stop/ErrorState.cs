using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate)
        {
            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - ErrorState:Ctor");
            this.parentStateMachine = parentStateMachine;
            this.axisToStop = axisToCalibrate;

            var messageData = new StopAxisMessageData(this.axisToStop);

            var errorNotification = new NotificationMessage(messageData, "Inverter operation error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.Stop, MessageStatus.OperationError, ErrorLevel.Error);
            parentStateMachine.PublishNotificationEvent(errorNotification);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            //Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - ErrorState:ProcessMessage");
            return false;
        }

        #endregion
    }
}
