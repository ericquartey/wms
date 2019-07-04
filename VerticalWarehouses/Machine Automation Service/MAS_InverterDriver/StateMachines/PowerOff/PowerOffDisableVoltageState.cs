using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff
{
    public class PowerOffDisableVoltageState : InverterStateBase
    {
        #region Constructors

        public PowerOffDisableVoltageState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PowerOffDisableVoltageState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        public override void Start()
        {
            this.InverterStatus.CommonControlWord.EnableVoltage = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

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

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PowerOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            this.ParentStateMachine.ChangeState(new PowerOffEndState(this.ParentStateMachine, this.InverterStatus, this.Logger));

            return true;
        }

        #endregion
    }
}
