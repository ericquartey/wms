using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        #endregion

        #region Constructors

        public PositioningEndState(IStateMachine parentMachine)
        {
            this.parentStateMachine = parentMachine;

            var positioninigData = ((IPositioningStateMachine)this.parentStateMachine).PositioningData;
            this.axisMovement = positioninigData.AxisMovement;

            //TEMP Send a message to stop the homing to the inverter (is it useful?)
            var inverterMessage = new CommandMessage(null,
                string.Format("Positioning {0} Stop", this.axisMovement),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop, //TEMP or MessageType.Homing
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end operation
            var newMessage = new NotificationMessage(null,
                string.Format("End Positioning {0}", this.axisMovement),
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,  //TEMP or MessageType.Homing
                MessageStatus.OperationEnd,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.parentStateMachine.PublishNotificationMessage(newMessage);
        }

        #endregion

        #region Properties

        public override string Type => string.Format("PositioningEndState {0}", this.axisMovement);

        #endregion

        #region Methods

        public override void SendCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void SendNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            {
                this.ProcessErrorOperation(message);
            }
        }

        private void ProcessErrorOperation(NotificationMessage message)
        {
            message.Destination = MessageActor.Any;

            //TEMP Send a notification about the error
            this.parentStateMachine.PublishNotificationMessage(message);
        }

        #endregion
    }
}
