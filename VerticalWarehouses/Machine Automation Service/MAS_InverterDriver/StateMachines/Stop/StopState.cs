using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Stop
{
    public class StopState : InverterStateBase
    {
        #region Fields

        private const ushort STATUS_WORD_VALUE = 0x0050;

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        private bool disposed;

        #endregion

        #region Constructors

        public StopState(IInverterStateMachine parentStateMachine, Axis axisToStop, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.axisToStop = axisToStop;

            this.logger.LogDebug($"2:Axis to stop{this.axisToStop}");

            switch (this.axisToStop)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8000;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0000;
                    break;
            }
            var commandMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            this.logger.LogTrace($"3:Stop message={commandMessage}");

            parentStateMachine.EnqueueMessage(commandMessage);

            var notificationMessageData = new ResetInverterFieldMessageData(this.axisToStop);
            var notificationMessage = new FieldNotificationMessage(notificationMessageData,
                $"Reset Inverter Axis {this.axisToStop}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterReset,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"4:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            parentStateMachine.PublishNotificationEvent(notificationMessage);

            this.logger.LogDebug("5:Method End");
        }

        #endregion

        #region Destructors

        ~StopState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.axisToStop, this.logger));
            }
            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:StatusWordValue={STATUS_WORD_VALUE}");

                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
                {
                    var messageData = new ResetInverterFieldMessageData(this.axisToStop);
                    var endNotification = new FieldNotificationMessage(messageData,
                        $"Reset Inverter Axis {this.axisToStop} completed",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterReset,
                        MessageStatus.OperationEnd);

                    this.logger.LogTrace($"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                    this.ParentStateMachine.PublishNotificationEvent(endNotification);
                    returnValue = true;
                }
            }

            this.logger.LogDebug("5:Method End");

            return returnValue;
        }

        /// <inheritdoc />
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
