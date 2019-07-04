using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.Template
{
    public class TemplateErrorState : StateBase
    {
        #region Fields

        private readonly Axis currentAxis;

        private readonly FieldNotificationMessage errorMessage;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateErrorState(
            IStateMachine parentMachine,
            Axis currentAxis,
            FieldNotificationMessage errorMessage,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.currentAxis = currentAxis;
            this.errorMessage = errorMessage;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~TemplateErrorState()
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

            if (message.Type == FieldMessageType.InverterPowerOff && message.Status != MessageStatus.OperationStart)
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
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            //TODO Identify Operation Target Inverter
            var stopMessageData = new InverterStopFieldMessageData(InverterIndex.MainInverter);
            var stopMessage = new FieldCommandMessage(
                stopMessageData,
                $"Reset Inverter Axis {this.currentAxis}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterPowerOff);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
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
