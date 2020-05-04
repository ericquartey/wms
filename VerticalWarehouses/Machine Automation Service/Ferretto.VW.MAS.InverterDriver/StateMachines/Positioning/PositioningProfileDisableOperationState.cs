using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningProfileDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningProfileDisableOperationState(
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
            this.Logger.LogDebug($"Positioning Profile Disable Operation.");
            this.Inverter.PositionControlWord.NewSetPoint = false;
            this.Inverter.PositionControlWord.RelativeMovement = false;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.InverterStatus.CommonControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

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
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.Inverter.PositionStatusWord.SetPointAcknowledge)
                {
                    // change target position to go back to starting position
                    int currentPosition = 0;
                    if (this.Inverter is AngInverterStatus angInverter)
                    {
                        currentPosition = angInverter.CurrentPositionAxisHorizontal;
                    }
                    else if (this.Inverter is AcuInverterStatus acuInverter)
                    {
                        currentPosition = acuInverter.CurrentPosition;
                    }

                    this.data.TargetPosition = this.data.StartPosition;

                    this.data.MovementType = MovementType.Absolute;
                    this.Logger.LogDebug($"Absolute Target changed from current position {currentPosition} to {this.data.TargetPosition}");
                    this.data.IsProfileCalibrateDone = true;

                    this.ParentStateMachine.ChangeState(
                        new PositioningSetParametersState(
                            this.ParentStateMachine,
                            this.data,
                            this.Inverter,
                            this.Logger));

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
