using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
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
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
            this.Inverter = inverterStatus;
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("Inverter Enable Operation");

            this.Inverter.PositionControlWord.HorizontalAxis = (this.data.AxisMovement == Axis.Horizontal);
            this.Inverter.PositionControlWord.EnableOperation = true;
            this.Inverter.PositionControlWord.RelativeMovement = (this.data.MovementType == MovementType.Relative);

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
                    this.Inverter.PositionControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(
                new PositioningEndState(
                    this.ParentStateMachine,
                    this.Inverter,
                    this.Logger,
                    true));
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
                this.ParentStateMachine.ChangeState(
                    new PositioningErrorState(
                        this.ParentStateMachine,
                        this.InverterStatus,
                        this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");

                if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (this.data.IsTorqueCurrentSamplingEnabled)
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningStartSamplingWhileMovingState(
                                this.data,
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningStartMovingState(
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }

                    returnValue = true;
                }
            }
            return returnValue;
        }

        #endregion
    }
}
