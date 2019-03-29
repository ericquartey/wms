using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownEndState : StateBase
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        #endregion

        #region Constructors

        public UpDownEndState(IStateMachine parentMachine, IUpDownRepetitiveMessageData upDownMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.upDownMessageData = upDownMessageData;

            //TEMP Send a message to stop to the inverter
            var inverterMessage = new CommandMessage(null,
                "Up&Down Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end (/stop) operation to all the world
            var newMessage = new NotificationMessage(null,
                "End Up&Down",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.parentStateMachine.OnPublishNotification(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "UpDownEndState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            {
                //TEMP Send a notification about the error
                this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine, this.upDownMessageData));
            }
        }

        #endregion
    }
}
