using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages.Data;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownEndState : StateBase
    {
        #region Constructors

        public UpDownEndState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var upDownMessageData = ((IUpDownRepetitiveStateMachine)this.parentStateMachine).UpDownRepetitiveData;
            var nRequiredCycles = upDownMessageData.NumberOfRequiredCycles;
            var numberOfCompletedCycles = ((IUpDownRepetitiveStateMachine)this.parentStateMachine).NumberOfCompletedCycles;

            //TEMP Send a message to stop the inverter (is it useful?)
            var inverterMessage = new CommandMessage(null,
                "Positioning Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end/stop operation
            string description;
            MessageStatus msgStatus;
            if (((IUpDownRepetitiveStateMachine)this.parentStateMachine).IsStopRequested)
            {
                description = "Up&Down Stop";
                msgStatus = MessageStatus.OperationStop;
            }
            else
            {
                description = "Up&Down End operation";
                msgStatus = MessageStatus.OperationEnd;
            }

            var message = new UpDownRepetitiveNotificationMessageData(numberOfCompletedCycles);
            var newMessage = new NotificationMessage(message,
                description,
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.UpDown,  //TEMP or MessageType.Homing
                msgStatus,
                ErrorLevel.NoError,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "UpDownEndState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            {
                this.ProcessErrorOperation(message);
            }
        }

        private void ProcessErrorOperation(NotificationMessage message)
        {
            //message.Destination = MessageActor.Any;

            //TEMP Send a notification about the error
            //this.parentStateMachine.PublishNotificationMessage(message);

            this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine), null);
        }

        #endregion
    }
}
