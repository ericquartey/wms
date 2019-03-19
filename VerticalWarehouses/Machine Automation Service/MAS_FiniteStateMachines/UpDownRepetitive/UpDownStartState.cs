using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownStartState : StateBase
    {
        #region Fields

        private readonly int nRequiredCycles;

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        #endregion

        #region Constructors

        public UpDownStartState(IStateMachine parentMachine, IUpDownRepetitiveMessageData upDownMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.upDownMessageData = upDownMessageData;

            this.nRequiredCycles = this.upDownMessageData.NumberOfRequiredCycles;

            var target = this.upDownMessageData.TargetLowerBound;
            //TEMP Retrieve parameters from DataLayer i.e. var speed = this.data.GetSpeedValue();
            var speed = 0.0m;
            var acceleration = 0.0m;
            var deceleration = 0.0m;

            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, target, speed, acceleration, deceleration);
            //TEMP send a message to start the positioning (to inverter and other components) toward
            var newMessage = new CommandMessage(messageData,
                "Up&Down start state",
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "UpDownStartState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Change to up&down end state (a request of stop operation has been made)
                    this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine, this.upDownMessageData));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
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
                        this.parentStateMachine.ChangeState(new UpDownErrorState(this.parentStateMachine, this.upDownMessageData));
                        break;

                    default:
                        break;
                }
            }
        }

        private void ProcessEndPositioning(NotificationMessage message)
        {
            if (this.parentStateMachine.OperationDone)
            {
                //TEMP Change to up&down end state
                this.parentStateMachine.ChangeState(new UpDownEndState(this.parentStateMachine, this.upDownMessageData));
            }
            else
            {
                //TEMP Change to the Up state
                this.parentStateMachine.ChangeState(new UpState(this.parentStateMachine, this.upDownMessageData));
            }
        }

        #endregion
    }
}
