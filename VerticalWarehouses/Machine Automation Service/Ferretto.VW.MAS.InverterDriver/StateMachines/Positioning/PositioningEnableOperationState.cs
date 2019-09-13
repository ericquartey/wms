using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningEnableOperationState : InverterStateBase
    {
        #region Fields

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
        }

        #endregion

        #region Destructors

        ~PositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.InverterStatus is AngInverterStatus currentStatus)
            {
                //INFO Set the axis to move in the CW
                currentStatus.PositionControlWord.HorizontalAxis = (this.data.AxisMovement == Axis.Horizontal);
                currentStatus.PositionControlWord.RelativeMovement = (this.data.MovementType == MovementType.Relative || this.data.MovementType == MovementType.TableTarget);
                currentStatus.PositionControlWord.EnableOperation = true;
            }

            this.Logger.LogDebug("Inverter Enable Operation");

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AngInverterStatus)this.InverterStatus).PositionControlWord.Value);
            this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

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
            this.Logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
            {
                if (this.data.IsTorqueCurrentSamplingEnabled)
                {
                    this.ParentStateMachine.ChangeState(
                        new PositioningStartSamplingWhileMovingState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.ParentStateMachine.ChangeState(
                        new PositioningStartMovingState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }

                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}
