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

        private readonly int numberExecutedSteps;

        private readonly IPositioningMessageData positioningMessageData;

        private bool disposed;

        private FieldCommandMessage stopMessage;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
            this.errorMessage = errorMessage;
            this.numberExecutedSteps = this.numberExecutedSteps;
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
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            PositioningMessageData messageData = null;

            if (message.Data is PositioningFieldMessageData data)
            {
                messageData = new PositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed,
                    data.TargetAcceleration, data.TargetDeceleration, 0, this.positioningMessageData.LowerBound, this.verticalPositioningMessageData.UpperBound,
                    this.positioningMessageData.Resolution, data.Verbosity);
            }
            var notificationMessage = new NotificationMessage(
                messageData,
                this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped due to an error" : "Belt Burninshing Stopped due to an error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            if (this.verticalPositioningMessageData.NumberCycles == 0)
            {
                this.stopMessage = new FieldCommandMessage(null,
                    $"Reset Inverter Axis {this.verticalPositioningMessageData.AxisMovement}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.Positioning);
            }
            else
            {
                this.stopMessage = new FieldCommandMessage(null,
                    $"Reset Inverter Belt Burninshing",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop);
            }

            this.logger.LogTrace($"1:Publish Field Command Message processed: {this.stopMessage.Type}, {this.stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(this.stopMessage);
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
