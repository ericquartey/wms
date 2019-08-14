using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningDisableOperationState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
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
            this.InverterStatus.CommonControlWord.EnableOperation = false;
            this.Logger.LogDebug("Disable Operation");

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger, true));
        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus is AglInverterStatus currentStatus)
            {
                if (!this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    this.ParentStateMachine.ChangeState(new ShutterPositioningDisableVoltageState(this.ParentStateMachine, this.InverterStatus, this.shutterPositionData, this.Logger));
                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
