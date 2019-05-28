using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningStartMovingState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        private bool positioningReachedReset;

        #endregion

        #region Constructors

        public PositioningStartMovingState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.logger = logger;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~PositioningStartMovingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            if (this.inverterStatus is AngInverterStatus currentStatus)
            {
                currentStatus.PositionControlWord.NewSetPoint = true;
            }
            //TODO complete type failure check

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.inverterStatus).PositionControlWord.Value);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            this.logger.LogDebug("3:Method End");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus is AngInverterStatus currentStatus)
            {
                if (!currentStatus.PositionStatusWord.PositioningAttained)
                {
                    this.positioningReachedReset = true;
                }
                if (this.positioningReachedReset && currentStatus.PositionStatusWord.PositioningAttained)
                {
                    this.ParentStateMachine.ChangeState(new PositioningDisableOperationState(this.ParentStateMachine, this.inverterStatus, this.logger));
                    returnValue = true;
                }
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
