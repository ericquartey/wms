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

            this.ParentStateMachine = parentStateMachine;
            this.logger = logger;
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
            var stopMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            this.logger.LogTrace($"3:Stop message={stopMessage}");

            parentStateMachine.EnqueueMessage(stopMessage);
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
                    var endNotification = new FieldNotificationMessage(messageData, "Axis calibration complete", FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver, FieldMessageType.InverterReset, MessageStatus.OperationEnd);

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
            this.logger.LogTrace($"1:Function Start");
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
