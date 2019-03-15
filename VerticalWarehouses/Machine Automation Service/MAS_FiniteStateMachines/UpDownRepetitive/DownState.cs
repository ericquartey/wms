using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages.Data;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class DownState : StateBase
    {
        #region Constructors

        public DownState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            //TEMP Send a notification message about the progress of procedure
            var numberOfCompletedCycles = ((IUpDownRepetitiveStateMachine)this.parentStateMachine).NumberOfCompletedCycles;

            var upDownMessage = new UpDownRepetitiveNotificationMessageData(numberOfCompletedCycles);
            var notifyMessage = new NotificationMessage(upDownMessage,
                "Up&Down running",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.UpDown,
                MessageStatus.OperationRunning);
            this.parentStateMachine.PublishNotificationMessage(notifyMessage);

            // Go to the UP
            var upDownMessageData = ((IUpDownRepetitiveStateMachine)this.parentStateMachine).UpDownRepetitiveData;
            var target = upDownMessageData.TargetUpperBound;
            //var speed = this.data.GetSpeedValue();
            var speed = 0.0m;
            var acceleration = 0.0m;
            var deceleration = 0.0m;

            var positioningData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, target, speed, acceleration, deceleration);
            //TEMP send a message to start the positioning (to inverter and other components) toward upper position
            var commandMessage = new CommandMessage(positioningData,
                "Positioning Up",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(commandMessage);
        }

        #endregion

        #region Properties

        public override string Type => "DownState";

        #endregion

        #region Methods

        public override void SendCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action
                    this.ProcessStopPositioning(message);
                    break;

                default:
                    break;
            }
        }

        public override void SendNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP the positioning operation is done successfully
                        this.ProcessEndPositioning(message);
                        break;

                    case MessageStatus.OperationError:
                        //TEMP an error occurs
                        this.ProcessErrorPositioning(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ProcessEndPositioning(NotificationMessage message)
        {
            //TEMP The positioning operation has been done

            // change to the Up state
            this.parentStateMachine.ChangeState(new UpState(this.parentStateMachine), null);
        }

        private void ProcessErrorPositioning(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine), null);
        }

        private void ProcessStopPositioning(CommandMessage message)
        {
            //TEMP This is a request to stop the operation of positioning
            //var newMessage = new CommandMessage(null,
            //    "Stop Requested",
            //    MessageActor.InverterDriver,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.Stop,
            //    MessageVerbosity.Info);

            ((IUpDownRepetitiveStateMachine)this.parentStateMachine).IsStopRequested = true;

            this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine), null);
        }

        #endregion
    }
}
