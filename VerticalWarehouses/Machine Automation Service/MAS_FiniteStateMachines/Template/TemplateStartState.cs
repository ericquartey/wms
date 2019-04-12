using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Template
{
    public class TemplateStartState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStartState(IStateMachine parentMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;

            var commandMessageData = new SwitchAxisFieldMessageData(this.axisToCalibrate);
            var commandMessage = new FieldCommandMessage(commandMessageData,
                $"Switch Axis {this.axisToCalibrate}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.logger.LogTrace($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new HomingMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Destructors

        ~TemplateStartState()
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
            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        //this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToCalibrate, message, this.logger));
                        break;
                }
            }
            this.logger.LogDebug("4:Method End");
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

            this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.axisToCalibrate, this.logger, true));

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
