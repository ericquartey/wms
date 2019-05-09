using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            this.logger = logger;
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~PositioningErrorState()
        {
            this.Dispose(false);
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
            //if (message.Data is PositioningFieldMessageData data)
            //{
            //    messageData = new PositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed,
            //        data.TargetAcceleration, data.TargetDeceleration, data.NumberCycles, this.positioningMessageData.LowerBound,
            //        this.positioningMessageData.UpperBound, data.Verbosity);
            //}
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
