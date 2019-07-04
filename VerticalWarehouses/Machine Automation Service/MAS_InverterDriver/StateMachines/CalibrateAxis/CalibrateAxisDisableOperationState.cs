using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;

using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public CalibrateAxisDisableOperationState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Destructors

        ~CalibrateAxisDisableOperationState()
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
            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.HomingControlWord.HomingOperation = false;
                currentStatus.HomingControlWord.EnableOperation = false;
            }

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).HomingControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (!this.InverterStatus.CommonStatusWord.IsOperationEnabled)
            {
                this.ParentStateMachine.ChangeState(new CalibrateAxisEndState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}
