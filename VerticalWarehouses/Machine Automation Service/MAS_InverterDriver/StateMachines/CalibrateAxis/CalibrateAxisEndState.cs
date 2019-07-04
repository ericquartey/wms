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
    public class CalibrateAxisEndState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public CalibrateAxisEndState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~CalibrateAxisEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate);
            var endNotification = new FieldNotificationMessage(
                messageData,
                "Axis calibration complete",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
