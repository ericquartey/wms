using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStartState : StateBase
    {
        #region Constructors

        public HomingStartState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var calibrateData = ((IHomingStateMachine)this.parentStateMachine).CalibrateData;

            //TEMP send a message to start the homing (to inverter and other components)
            var newMessage = new CommandMessage(calibrateData,
                "Homing State Started",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Calibrate, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "HomingStartState";

        #endregion

        #region Methods

        public override void SendCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.StopHoming:
                    //TODO add state business logic to stop current action
                    this.ProcessStopHoming(message);
                    break;

                default:
                    break;
            }
        }

        public override void SendNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Homing)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP the homing operation is done successfully
                        this.ProcessEndHoming(message);
                        break;

                    case MessageStatus.OperationError:
                        //TEMP an error occurs
                        this.ProcessErrorHoming(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ProcessEndHoming(NotificationMessage message)
        {
            //TEMP The homing operation has been done
            var newMessage = new CommandMessage(null,
                "End Homing",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,
                MessageVerbosity.Info);

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine), null);
        }

        private void ProcessErrorHoming(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new HomingErrorState(this.parentStateMachine), null);
        }

        private void ProcessStopHoming(CommandMessage message)
        {
            //TEMP This is a request to stop the operation
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.StopHoming,
                MessageVerbosity.Info);

            this.parentStateMachine.ChangeState(new HomingEndState(this.parentStateMachine), null);
        }

        #endregion
    }
}
