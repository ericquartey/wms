using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingEndState : StateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingEndState(IStateMachine parentMachine, Axis axisToStop, ILogger logger, bool stopRequested = false)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.stopRequested = stopRequested;
            this.ParentStateMachine = parentMachine;
            this.axisToStop = axisToStop;

            var stopMessageData = new ResetInverterFieldMessageData(this.axisToStop);
            var stopMessage = new FieldCommandMessage(stopMessageData,
                $"Reset Inverter Axis {this.axisToStop}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterReset);

            this.logger.LogTrace($"2:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~HomingEndState()
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

            if (message.Type == FieldMessageType.InverterReset)
            {
                var notificationMessageData = new HomingMessageData(this.axisToStop, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Homing,
                    this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

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
