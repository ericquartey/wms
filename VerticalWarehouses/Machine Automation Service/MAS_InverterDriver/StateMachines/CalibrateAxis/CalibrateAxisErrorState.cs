using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateAxisErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Destructors

        ~CalibrateAxisErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
            throw new System.NotImplementedException();
        }

        public override void Start()
        {
            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate);

            var errorNotification = new FieldNotificationMessage(
                messageData,
                "Inverter operation error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.logger.LogTrace($"1:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(errorNotification);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
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
