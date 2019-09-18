using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartMovingState : InverterStateBase
    {
        #region Constructors

        public PositioningStartMovingState(
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

        protected bool TargetPositionReached =>
            this.Inverter.PositionStatusWord.SetPointAcknowledge
            &&
            this.Inverter.PositionStatusWord.PositioningAttained;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"PositioningStartMoving.Start Inverter type={this.InverterStatus.GetType().Name}");

            this.Inverter.PositionControlWord.NewSetPoint = true;

            this.Logger.LogDebug("Set New Setpoint");

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
                    this.Inverter.PositionControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(
                new PositioningEndState(
                    this.ParentStateMachine,
                    this.Inverter,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.ParameterId == InverterParameterId.ControlWordParam)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(
                    new PositioningErrorState(
                        this.ParentStateMachine,
                        this.InverterStatus,
                        this.Logger));

                return true;
            }

            this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");

            if (this.TargetPositionReached)
            {
                this.ParentStateMachine.ChangeState(
                    new PositioningDisableOperationState(
                        this.ParentStateMachine,
                        this.Inverter,
                        this.Logger));

                this.Logger.LogDebug("Target position reached.");
            }
            else
            {
                this.Logger.LogDebug("Moving towards target position.");
            }

            return true; //INFO Next status word request handled by timer
        }

        #endregion
    }
}
