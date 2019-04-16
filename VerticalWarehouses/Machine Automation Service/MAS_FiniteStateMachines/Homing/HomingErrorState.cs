using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingErrorState : StateBase
    {
        #region Fields

        private readonly Axis currentAxis;

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingErrorState(IStateMachine parentMachine, Axis currentAxis, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentMachine;
            this.currentAxis = currentAxis;
            this.errorMessage = errorMessage;

            var stopMessageData = new ResetInverterFieldMessageData(this.currentAxis);
            var stopMessage = new FieldCommandMessage(stopMessageData,
                $"Reset Inverter Axis {this.currentAxis}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterReset);

            this.logger.LogTrace($"2:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~HomingErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
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

            if (message.Type == FieldMessageType.InverterReset && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new HomingMessageData(this.currentAxis, MessageVerbosity.Error);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Stopped due to an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Homing,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                this.logger.LogDebug("3:Method End");
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogDebug("2:Method End");
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
