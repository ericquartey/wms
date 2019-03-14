using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStartState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        #endregion

        #region Constructors

        public PositioningStartState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var positioninigData = ((IPositioningStateMachine)this.parentStateMachine).PositioningData;
            this.axisMovement = positioninigData.AxisMovement;

            //TEMP send a message to start the positioning (to inverter and other components)
            var newMessage = new CommandMessage(positioninigData,
                string.Format("Positioning {0} State Started", this.axisMovement),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => "PositioningStartState";

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
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

            this.parentStateMachine.ChangeState(new PositioningEndState(this.parentStateMachine), null);
        }

        private void ProcessErrorPositioning(NotificationMessage message)
        {
            this.parentStateMachine.ChangeState(new PositioningErrorState(this.parentStateMachine), null);
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

            this.parentStateMachine.ChangeState(new PositioningEndState(this.parentStateMachine), null);
        }

        #endregion
    }
}
