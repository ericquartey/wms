using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingEndState : StateBase
    {
        #region Constructors

        public HomingEndState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            //TEMP Send a message to stop the homing to the inverter (is it useful?)
            var inverterMessage = new CommandMessage(null,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end operation
            var newMessage = new NotificationMessage(null,
                "End Homing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,  //TEMP or MessageType.Homing
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingEndState";

        #endregion

        #region Methods

        public override void MakeOperation()
        {
            throw new NotImplementedException();
        }

        public override void SendCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void SendNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Homing && message.Status == MessageStatus.OperationError)
            {
                this.ProcessErrorOperation(message);
            }
        }

        public override void Stop()
        {
            throw new NotImplementedException();
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
