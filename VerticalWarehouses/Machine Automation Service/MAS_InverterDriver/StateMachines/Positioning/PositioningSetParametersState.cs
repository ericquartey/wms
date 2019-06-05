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

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningSetParametersState(IInverterStateMachine parentStateMachine, IInverterPositioningFieldMessageData data, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.data = data;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Destructors

        ~PositioningSetParametersState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.logger.LogTrace("1:Method Start");

            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPositionParam, this.data.TargetPosition));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.logger.LogTrace($"2:message={message}:ID Parametro={message.ParameterId}");

            switch (message.ParameterId)
            {
                case InverterParameterId.PositionTargetPositionParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeedParam, this.data.TargetSpeed));
                    break;

                case InverterParameterId.PositionTargetSpeedParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionAccelerationParam, this.data.TargetAcceleration));
                    break;

                case InverterParameterId.PositionAccelerationParam:
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionDecelerationParam, this.data.TargetDeceleration));
                    break;

                case InverterParameterId.PositionDecelerationParam:
                    this.ParentStateMachine.ChangeState(new PositioningEnableOperationState(this.ParentStateMachine, this.data, this.inverterStatus, this.logger));
                    break;
            }

            return returnValue;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
