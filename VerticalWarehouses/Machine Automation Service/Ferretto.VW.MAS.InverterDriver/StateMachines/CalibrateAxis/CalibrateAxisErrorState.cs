﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal class CalibrateAxisErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public CalibrateAxisErrorState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Calibrate error state axis {this.axisToCalibrate}");
            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate);

            var errorNotification = new FieldNotificationMessage(
                messageData,
                "Inverter operation error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationError,
                this.InverterStatus.SystemIndex,
                ErrorLevel.Error);

            this.Logger.LogTrace($"1:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(errorNotification);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Stop ignored in error state");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;   // EvaluateWriteMessage will not send a StatusWordParam
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;    // EvaluateReadMessage will stop sending StatusWordParam
        }

        #endregion
    }
}
