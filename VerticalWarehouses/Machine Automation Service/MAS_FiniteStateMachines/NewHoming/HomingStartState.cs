using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines.NewHoming
{
    public class HomingStartState : StateBase
    {
        #region Constructors

        public HomingStartState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            // send a message to start the homing
            var newMessage = new CommandMessage(null,
                "Homing State Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Calibrate,  // or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);

            // send a message to start the homing to the inverter
            //var inverterMessage = new CommandMessage(null,
            //    "Homing State Started",
            //    MessageActor.InverterDriver,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.Calibrate, // or MessageType.Homing
            //    MessageVerbosity.Info);
            //this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingStartState";

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
                case MessageType.StopHoming:
                    //TODO add state business logic to stop current action
                    this.ProcessStopHoming(message);
                    break;

                case MessageType.EndAction:
                    //TODO add state business logic to stop current action
                    this.ProcessEndHoming(message);
                    break;

                case MessageType.ErrorAction:
                    this.ProcessErrorHoming(message);
                    break;

                default:
                    break;
            }
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        private void ProcessEndHoming(CommandMessage message)
        {
            // The homing operation has been done
            var newMessage = new CommandMessage(null,
                "End Homing",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,
                MessageVerbosity.Info);

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine), newMessage);
        }

        private void ProcessErrorHoming(CommandMessage message)
        {
            // The homing operation has been done
            var newMessage = new CommandMessage(null,
                "End Homing",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,
                MessageVerbosity.Info);

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine), newMessage);
        }

        private void ProcessStopHoming(CommandMessage message)
        {
            // This is a request to stop the operation
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,
                MessageVerbosity.Info);

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine), newMessage);
        }

        #endregion
    }
}
