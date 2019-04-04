using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.logger = logger;

            var messageData = new CalibrateAxisFieldMessageData(axisToCalibrate);

            var errorNotification = new FieldNotificationMessage(messageData, "Inverter operation error", FieldMessageActor.Any,
                FieldMessageActor.InverterDriver, FieldMessageType.CalibrateAxis, MessageStatus.OperationError, ErrorLevel.Error);

            this.logger.LogTrace($"2:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            parentStateMachine.PublishNotificationEvent(errorNotification);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~ErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}");

            return false;
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
