using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            this.logger = logger;
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.errorMessage = errorMessage;

            var stopMessageData = new ResetInverterFieldMessageData(this.positioningMessageData.AxisMovement);
            var stopMessage = new FieldCommandMessage(stopMessageData,
                $"Reset Inverter Axis {this.positioningMessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning);

            this.logger.LogTrace($"2:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            PositioningMessageData messageData = null;
            if (message.Data is PositioningFieldMessageData data)
            {
                messageData = new PositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed, data.TargetAcceleration, data.TargetDeceleration, data.Verbosity);
            }
            var notificationMessage = new NotificationMessage(
                messageData,
                "Positioning Stopped due to an error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");
        }

        #endregion
    }
}
