using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        #endregion

        #region Constructors

        public CalibrateAxisStartState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            Calibration calibration,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogInformation($"Calibrate start axis {this.axisToCalibrate} inverter {this.InverterStatus.SystemIndex}");
            this.InverterStatus.OperatingMode = (ushort)InverterOperationMode.Homing;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.SetOperatingMode,
                    this.InverterStatus.OperatingMode));

            this.ParentStateMachine.PublishNotificationEvent(
                new FieldNotificationMessage(
                    new CalibrateAxisFieldMessageData(this.axisToCalibrate, this.calibration, MessageVerbosity.Info),
                $"{this.axisToCalibrate} Homing started",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Calibrate Stop requested");

            this.ParentStateMachine.ChangeState(
                new CalibrateAxisDisableOperationState(
                    this.ParentStateMachine,
                    this.axisToCalibrate,
                    this.calibration,
                    this.InverterStatus,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (message.ParameterId == InverterParameterId.SetOperatingMode)
                {
                    this.ParentStateMachine.ChangeState(
                        new CalibrateAxisSetParametersState(
                            this.ParentStateMachine,
                            this.axisToCalibrate,
                            this.calibration,
                            this.InverterStatus,
                            this.Logger));

                    var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate, this.calibration, MessageVerbosity.Info);
                    var notificationMessage = new FieldNotificationMessage(
                        messageData,
                        $"{this.axisToCalibrate} calibrate axis executing",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.CalibrateAxis,
                        MessageStatus.OperationExecuting,
                        this.InverterStatus.SystemIndex);

                    this.Logger.LogTrace($"2:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

                    this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
                }
            }

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
