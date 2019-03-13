using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class EndState : IoStateBase
    {
        #region Constructors

        public EndState(Axis axisToSwitchOn, IIoStateMachine parentStateMachine)
        {
            Console.WriteLine($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - EndState:Ctor");
            this.parentStateMachine = parentStateMachine;

            SwitchAxisMessageData messageData = new SwitchAxisMessageData(axisToSwitchOn);
            var endNotification = new NotificationMessage(messageData, "Motor Switch complete", MessageActor.Any,
                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);
            this.parentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
        }

        #endregion
    }
}
