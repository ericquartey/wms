using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartSamplingWhileMovingState : InverterStateBase
    {
        #region Constructors

        public PositioningStartSamplingWhileMovingState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.PositionControlWord.NewSetPoint = true;
            }
            //TODO complete type failure check
            this.Logger.LogDebug("Set New Setpoint");

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).PositionControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.InverterStatus, this.Logger, true));
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
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                if (currentStatus.PositionStatusWord.SetPointAcknowledge && currentStatus.PositionStatusWord.PositioningAttained)
                {
                    this.ParentStateMachine.ChangeState(new PositioningDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    this.Logger.LogDebug("Position Reached !");
                }
                else
                {
                    this.Logger.LogDebug("Position Not Reached");
                }
            }

            //INFO Next status word request handled by timer
            return true;
        }

        protected override void OnDisposing()
        {
        }

        #endregion
    }
}
