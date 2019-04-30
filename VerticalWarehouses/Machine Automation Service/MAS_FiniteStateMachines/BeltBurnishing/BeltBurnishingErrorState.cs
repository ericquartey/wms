using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.BeltBurnishing
{
    public class BeltBurnishingErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly int numberExecutedSteps;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly FieldCommandMessage stopMessage;

        private readonly ResetInverterFieldMessageData stopMessageData;

        #endregion

        #region Constructors

        public BeltBurnishingErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            this.logger = logger;
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.errorMessage = errorMessage;
            this.numberExecutedSteps = this.numberExecutedSteps;

            if (positioningMessageData.NumberCycles == 0)
            {
                this.stopMessageData = new ResetInverterFieldMessageData(this.positioningMessageData.AxisMovement);
                this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                    $"Reset Inverter Axis {this.positioningMessageData.AxisMovement}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else
            {
                this.stopMessageData = new ResetInverterFieldMessageData(this.positioningMessageData.NumberCycles);
                this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                    $"Reset Inverter belt break-in",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterReset);
            }

            this.logger.LogTrace($"2:Publish Field Command Message processed: {this.stopMessage.Type}, {this.stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.stopMessage);

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
                messageData = new PositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed,
                    data.TargetAcceleration, data.TargetDeceleration, 0, this.positioningMessageData.LowerBound, this.positioningMessageData.UpperBound,
                    data.Verbosity);
            }
            var notificationMessage = new NotificationMessage(
                messageData,
                this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped due to an error" : "Belt Break-In Stopped due to an error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.BeltBurnishing,
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
