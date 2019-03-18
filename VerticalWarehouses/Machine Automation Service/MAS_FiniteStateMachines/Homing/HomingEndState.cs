using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingEndState : StateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        #endregion

        #region Constructors

        public HomingEndState(IStateMachine parentMachine, Axis axisToStop)
        {
            this.parentStateMachine = parentMachine;
            this.axisToStop = axisToStop;

            //TEMP Send a message to stop the homing to the inverter
            var stopMessageData = new StopAxisMessageData(this.axisToStop);
            var inverterMessage = new CommandMessage(stopMessageData,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            var messageStatus = ((IHomingStateMachine)this.parentStateMachine).IsStopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd;

            //TEMP Send a notification about the end (/stop) operation to all the world
            var newMessage = new NotificationMessage(null,
                "End Homing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,  //TEMP or MessageType.Homing
                messageStatus,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Homing && message.Status == MessageStatus.OperationError)
            {
                this.ProcessErrorOperation(message);
            }
        }

        private void ProcessErrorOperation(NotificationMessage message)
        {
            message.Destination = MessageActor.Any;

            //TEMP Send a notification about the error
            this.parentStateMachine.PublishNotificationMessage(message);
        }

        #endregion
    }
}
