using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    internal class PowerOnSwitchOnState : InverterStateBase
    {
        #region Constructors

        public PowerOnSwitchOnState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.InverterStatus.CommonControlWord.SwitchOn = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Power On Stop requested");

            this.ParentStateMachine.ChangeState(
                new PowerOnEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var responseReceived = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsSwitchedOn)
                {
                    this.ParentStateMachine.ChangeState(new PowerOnEndState(this.ParentStateMachine, this.InverterStatus, this.Logger));

                    responseReceived = true;
                }
            }

            return responseReceived;
        }

        #endregion
    }
}
