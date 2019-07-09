using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
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

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public CalibrateAxisEndState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Destructors

        ~CalibrateAxisEndState()
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
            if (this.stopRequested)
            {
                if (this.InverterStatus is AngInverterStatus currentStatus)
                {
                    currentStatus.HomingControlWord.HomingOperation = false;
                }
            }

            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate);
            var endNotification = new FieldNotificationMessage(
                messageData,
                "Axis calibration complete",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                (this.stopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
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
