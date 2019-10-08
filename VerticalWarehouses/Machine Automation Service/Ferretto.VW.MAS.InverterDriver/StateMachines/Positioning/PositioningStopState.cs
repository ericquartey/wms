using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStopState : InverterStateBase
    {
        #region Constructors

        public PositioningStopState(
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Inverter = inverterStatus;
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"Positioning Switch Off Inverter");
            this.Inverter.PositionControlWord.SwitchOn = false;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
                    this.InverterStatus.CommonControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Stop process already active");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsSwitchedOn)
                {
                    this.ParentStateMachine.ChangeState(
                        new PositioningDisableOperationState(
                            this.ParentStateMachine,
                            this.Inverter,
                            this.Logger,
                            true));

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
