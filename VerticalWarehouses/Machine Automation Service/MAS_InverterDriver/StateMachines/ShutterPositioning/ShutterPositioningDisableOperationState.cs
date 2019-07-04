using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningDisableOperationState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.shutterPositionData = shutterPositionData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningDisableOperationState()
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
            if (this.inverterStatus is AglInverterStatus currentStatus)
            {
                currentStatus.ProfileVelocityControlWord.EnableOperation = false;
            }

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AglInverterStatus)this.inverterStatus).CommonControlWord.Value);

            this.logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus is AglInverterStatus currentStatus)
            {
                if (!this.inverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    this.ParentStateMachine.ChangeState(new ShutterPositioningDisableVoltageState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
                    returnValue = true;
                }
            }

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
