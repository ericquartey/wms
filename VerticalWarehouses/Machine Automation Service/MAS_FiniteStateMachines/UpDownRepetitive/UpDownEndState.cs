using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;

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
            this.ParentStateMachine = parentMachine;
            this.upDownMessageData = upDownMessageData;

            //TEMP Send a message to stop to the inverter
            //var inverterMessage = new CommandMessage(null,
            //    "Up&Down Stop",
            //    MessageActor.InverterDriver,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.Stop,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end (/stop) operation to all the world
            var newMessage = new NotificationMessage(null,
                "End Up&Down",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.ParentStateMachine.PublishNotificationMessage(newMessage);
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

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            //{
            //    //TEMP Send a notification about the error
            //    this.ParentStateMachine.ChangeState(new UpDownErrorState(this.ParentStateMachine, this.upDownMessageData));
            //}
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
