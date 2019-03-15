using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpState : StateBase
    {
        #region Fields

        private readonly int nRequiredCycles;

        #endregion

        #region Constructors

        public UpState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            // Go to the Down

            var upDownMessageData = ((IUpDownRepetitiveStateMachine)this.parentStateMachine).UpDownRepetitiveData;
            this.nRequiredCycles = upDownMessageData.NumberOfRequiredCycles;

            var target = upDownMessageData.TargetLowerBound;
            //var speed = this.data.GetSpeedValue();
            var speed = 0.0m;
            var acceleration = 0.0m;
            var deceleration = 0.0m;

            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, target, speed, acceleration, deceleration);
            //TEMP send a message to start the positioning (to inverter and other components) toward
            var newMessage = new CommandMessage(messageData,
                "Up&Down start state",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "UpState";

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
            //TEMP var newMessage = new CommandMessage(null,
            //TEMP    string.Format("End Positioning {0}", this.axisMovement),
            //TEMP    MessageActor.InverterDriver,
            //TEMP    MessageActor.FiniteStateMachines,
            //TEMP    MessageType.StopPositioning,
            //TEMP    MessageVerbosity.Info);

            ((IUpDownRepetitiveStateMachine)this.parentStateMachine).NumberOfCompletedCycles++;
            if (((IUpDownRepetitiveStateMachine)this.parentStateMachine).NumberOfCompletedCycles == this.nRequiredCycles)
            {
                // the up&down procedure has been done completely
                ((IUpDownRepetitiveStateMachine)this.parentStateMachine).IsStopRequested = false;
                this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine), null);
            }
            else
            {
                // change to the Down state
                this.parentStateMachine.ChangeState(new DownState(this.parentStateMachine), null);
            }
        }

        private void ProcessErrorPositioning(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine), null);
        }

        private void ProcessStopPositioning(CommandMessage message)
        {
            //TEMP This is a request to stop the operation of positioning
            var newMessage = new CommandMessage(null,
                "Stop Requested",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);

            ((IUpDownRepetitiveStateMachine)this.parentStateMachine).IsStopRequested = true;

            this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine), null);
        }

        #endregion
    }
}
