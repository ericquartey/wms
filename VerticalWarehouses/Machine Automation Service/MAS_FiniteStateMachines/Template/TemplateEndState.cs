using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.Template
{
    public class TemplateEndState : StateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateEndState(IStateMachine parentMachine, Axis axisToStop, ILogger logger, bool stopRequested = false)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.stopRequested = stopRequested;
            this.ParentStateMachine = parentMachine;
            this.axisToStop = axisToStop;
        }

        #endregion

        #region Destructors

        ~TemplateEndState()
        {
            this.Dispose(false);
        }

        #endregion

        /// <inheritdoc/>

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterPowerOff:
                case FieldMessageType.CalibrateAxis:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            var notificationMessageData = new HomingMessageData(this.axisToStop, MessageVerbosity.Info);
                            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                                "Homing Completed",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.Homing,
                                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new TemplateErrorState(this.ParentStateMachine, this.axisToStop, message, this.logger));
                            break;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var notificationMessageData = new HomingMessageData(this.axisToStop, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.logger.LogTrace($"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
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
