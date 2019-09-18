using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningSetParametersState : InverterStateBase
    {

        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningSetParametersState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPositionParam, this.data.TargetPosition));
            this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
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
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:ID Parametro={message.ParameterId}");

                switch (message.ParameterId)
                {
                    case InverterParameterId.PositionTargetPositionParam:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeedParam, this.data.TargetSpeed[0]));
                        this.Logger.LogDebug($"Set target Speed: {this.data.TargetSpeed}");
                        break;

                    case InverterParameterId.PositionTargetSpeedParam:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionAccelerationParam, this.data.TargetAcceleration[0]));
                        this.Logger.LogDebug($"Set Acceleration: {this.data.TargetAcceleration}");
                        break;

                    case InverterParameterId.PositionAccelerationParam:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionDecelerationParam, this.data.TargetDeceleration[0]));
                        this.Logger.LogDebug($"Set Deceleration: {this.data.TargetDeceleration}");
                        break;

                    case InverterParameterId.PositionDecelerationParam:
                        this.ParentStateMachine.ChangeState(new PositioningEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus, this.Logger));
                        break;
                }
            }
            return returnValue;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
