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
    public class TemplateStartState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStartState(
            IStateMachine parentMachine,
            Axis axisToCalibrate,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.axisToCalibrate = axisToCalibrate;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~TemplateStartState()
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.axisToCalibrate, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new TemplateErrorState(this.ParentStateMachine, this.axisToCalibrate, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var commandMessageData = new SwitchAxisFieldMessageData(this.axisToCalibrate);
            var commandMessage = new FieldCommandMessage(
                commandMessageData,
                $"Switch Axis {this.axisToCalibrate}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new HomingMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.axisToCalibrate, this.Logger, true));
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
