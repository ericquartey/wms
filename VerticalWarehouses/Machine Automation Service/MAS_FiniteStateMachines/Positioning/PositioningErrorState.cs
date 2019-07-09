using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly IPositioningMessageData positioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningErrorState(
            IStateMachine parentMachine,
            IPositioningMessageData positioningMessageData,
            FieldNotificationMessage errorMessage,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.positioningMessageData = positioningMessageData;
            this.errorMessage = errorMessage;
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
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterStop && message.Status == MessageStatus.OperationError)
            {
                var notificationMessageData = new PositioningMessageData(
                    this.positioningMessageData.AxisMovement,
                this.positioningMessageData.MovementType,
                this.positioningMessageData.TargetPosition,
                this.positioningMessageData.TargetSpeed,
                this.positioningMessageData.TargetAcceleration,
                this.positioningMessageData.TargetDeceleration,
                0,
                this.positioningMessageData.LowerBound,
                this.positioningMessageData.UpperBound,
                this.positioningMessageData.Resolution,
                MessageVerbosity.Error);

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped due to an error" : "Belt Burnishing Stopped due to an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var description = this.positioningMessageData.NumberCycles == 0 ? $"Reset Inverter Axis {this.positioningMessageData.AxisMovement}" : $"Reset Inverter Belt Burninshing";
            var stopMessage = new FieldCommandMessage(
                null,
                    description,
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var notificationMessageData = new PositioningMessageData(
                this.positioningMessageData.AxisMovement,
                this.positioningMessageData.MovementType,
                this.positioningMessageData.TargetPosition,
                this.positioningMessageData.TargetSpeed,
                this.positioningMessageData.TargetAcceleration,
                this.positioningMessageData.TargetDeceleration,
                0,
                this.positioningMessageData.LowerBound,
                this.positioningMessageData.UpperBound,
                this.positioningMessageData.Resolution,
                MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                                    notificationMessageData,
                                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Error" : "Belt Burnishing Error",
                                    MessageActor.Any,
                                    MessageActor.FiniteStateMachines,
                                    MessageType.Positioning,
                                    MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
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
