using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {
        #region Fields

        private readonly Axis axisMovement;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningEndState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData)
        {
            this.parentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.axisMovement = positioningMessageData.AxisMovement;

            //TEMP Send a message to stop the homing to the inverter
            var inverterMessage = new CommandMessage(null,
                string.Format("Positioning {0} Stop", this.axisMovement),
                MessageActor.InverterDriver,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info);
            this.parentStateMachine.PublishCommandMessage(inverterMessage);

            //TEMP Send a notification about the end operation
            var newMessage = new NotificationMessage(null,
                string.Format("End Positioning {0}", this.axisMovement),
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

        public override string Type => string.Format("PositioningEndState {0}", this.axisMovement);

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
                //TEMP Publish a notification about the error
                this.parentStateMachine.PublishNotificationMessage(message);
            }
        }

        #endregion
    }
}
