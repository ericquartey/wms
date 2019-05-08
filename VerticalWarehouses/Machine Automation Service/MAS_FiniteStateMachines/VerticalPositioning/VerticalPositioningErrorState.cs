using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalPositioning
{
    public class VerticalPositioningErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly int numberExecutedSteps;

        private readonly FieldCommandMessage stopMessage;

        private readonly ResetInverterFieldMessageData stopMessageData;

        private readonly IVerticalPositioningMessageData verticalPositioningMessageData;

        #endregion

        #region Constructors

        public VerticalPositioningErrorState(IStateMachine parentMachine, IVerticalPositioningMessageData verticalPositioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            this.logger = logger;
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;
            this.verticalPositioningMessageData = verticalPositioningMessageData;
            this.errorMessage = errorMessage;
            this.numberExecutedSteps = this.numberExecutedSteps;

            if (verticalPositioningMessageData.NumberCycles == 0)
            {
                this.stopMessageData = new ResetInverterFieldMessageData(this.verticalPositioningMessageData.AxisMovement);
                this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                    $"Reset Inverter Axis {this.verticalPositioningMessageData.AxisMovement}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else
            {
                this.stopMessageData = new ResetInverterFieldMessageData(this.verticalPositioningMessageData.NumberCycles);
                this.stopMessage = new FieldCommandMessage(this.stopMessageData,
                    $"Reset Inverter Belt Burninshing",
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

            VerticalPositioningMessageData messageData = null;

            if (message.Data is PositioningFieldMessageData data)
            {
                messageData = new VerticalPositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed,
                    data.TargetAcceleration, data.TargetDeceleration, 0, this.verticalPositioningMessageData.LowerBound, this.verticalPositioningMessageData.UpperBound,
                    data.Verbosity);
            }
            var notificationMessage = new NotificationMessage(
                messageData,
                this.verticalPositioningMessageData.NumberCycles == 0 ? "Positioning Stopped due to an error" : "Belt Burninshing Stopped due to an error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.VerticalPositioning,
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
