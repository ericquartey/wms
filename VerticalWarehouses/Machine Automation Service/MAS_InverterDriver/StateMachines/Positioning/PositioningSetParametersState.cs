using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

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

        #region Destructors

        ~PositioningSetParametersState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue => this.inverterCommandQueue;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPositionParam, this.data.TargetPosition));
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

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            this.Logger.LogTrace($"2:message={message}:ID Parametro={message.ParameterId}");

            switch (message.ParameterId)
            {
                case InverterParameterId.PositionTargetPositionParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeedParam, this.data.TargetSpeed));
                    break;

                case InverterParameterId.PositionTargetSpeedParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionAccelerationParam, this.data.TargetAcceleration));
                    break;

                case InverterParameterId.PositionAccelerationParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionDecelerationParam, this.data.TargetDeceleration));
                    break;

                case InverterParameterId.PositionDecelerationParam:
                    this.ParentStateMachine.ChangeState(new PositioningEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus, this.Logger));
                    break;
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
