using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningEnableOperationState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                //INFO Set the axis to move in the CW
                currentStatus.PositionControlWord.HorizontalAxis = this.data.AxisMovement == Axis.Horizontal;
                currentStatus.PositionControlWord.RelativeMovement = this.data.MovementType == MovementType.Relative;
                currentStatus.PositionControlWord.EnableOperation = true;
            }

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).PositionControlWord.Value);
            this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
            {
                this.ParentStateMachine.ChangeState(new PositioningStartMovingState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}
