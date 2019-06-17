using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStartState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public HomingStartState(IStateMachine parentMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Destructors

        ~HomingStartState()
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
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToCalibrate, message, this.logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToCalibrate, message, this.logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            var ioCommandMessageData = new SwitchAxisFieldMessageData(this.axisToCalibrate);
            var ioCommandMessage = new FieldCommandMessage(ioCommandMessageData,
                $"Switch Axis {this.axisToCalibrate}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            //TODO Check if hard coding inverter index on MainInverter is correct or a dynamic selection of inverter index is required
            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.axisToCalibrate, InverterIndex.MainInverter);
            var inverterCommandMessage = new FieldCommandMessage(inverterCommandMessageData,
                $"Switch Axis {this.axisToCalibrate}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn);

            this.logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

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
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new HomingEndState(this.ParentStateMachine, this.axisToCalibrate, this.logger, true));
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
