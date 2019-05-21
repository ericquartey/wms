using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalPositioning
{
    public class VerticalPositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public VerticalPositioningEnableOperationState(IInverterStateMachine parentStateMachine, IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.data = data;
            this.inverterStatus = inverterStatus;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~VerticalPositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPositionParam, this.data.TargetPosition));

            this.logger.LogDebug("2:Method End");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            var returnValue = false;

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");
            this.logger.LogTrace($"3:message={message}:ID Parametro={message.ParameterId}");

            switch (message.ParameterId)
            {
                case (InverterParameterId.PositionTargetPositionParam):
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeedParam, this.data.TargetSpeed));
                    break;

                case (InverterParameterId.PositionTargetSpeedParam):
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionAccelerationParam, this.data.TargetAcceleration));
                    break;

                case (InverterParameterId.PositionAccelerationParam):
                    this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.PositionDecelerationParam, this.data.TargetDeceleration));
                    break;

                case (InverterParameterId.PositionDecelerationParam):
                    if (this.inverterStatus is AngInverterStatus currentStatus)
                    {
                        currentStatus.PositionControlWord.RelativeMovement = true;
                        currentStatus.PositionControlWord.EnableOperation = true;
                    }

                    var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.inverterStatus).PositionControlWord.Value);
                    this.logger.LogTrace($"4:inverterMessage={inverterMessage}");
                    this.ParentStateMachine.EnqueueMessage(inverterMessage);
                    returnValue = true;
                    break;
            }

            this.logger.LogDebug("6:Method End");

            return returnValue;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new VerticalPositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus.CommonStatusWord.IsOperationEnabled)
            {
                this.ParentStateMachine.ChangeState(new VerticalPositioningStartMovingState(this.ParentStateMachine, this.inverterStatus, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("3:Method End");

            return returnValue;
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
