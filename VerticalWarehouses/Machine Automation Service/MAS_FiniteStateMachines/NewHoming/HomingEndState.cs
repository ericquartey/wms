using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.NewHoming
{
    public class HomingEndState : StateBase
    {
        #region Constructors

        public HomingEndState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            // send a message to stop the homing to the inverter (it can be useless)
            var inverterMessage = new CommandMessage(null,
                "Homing Stop",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming, // or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            // Send a notification about the end operation
            var newMessage = new NotificationMessage(null,
                "End Homing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,  // or MessageType.Homing
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public string Type => "HomingEndState";

        #endregion

        #region Methods

        public override void MakeOperation()
        {
            throw new NotImplementedException();
        }

        public override void NotifyMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.StopAction:
                    //TODO add state business logic to stop current action

                    var newMessage = new CommandMessage(null,
                        "Stop Requested",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.StopAction,
                        MessageVerbosity.Info);
                    this.parentStateMachine.PublishCommandMessage(newMessage);
                    break;
            }
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
