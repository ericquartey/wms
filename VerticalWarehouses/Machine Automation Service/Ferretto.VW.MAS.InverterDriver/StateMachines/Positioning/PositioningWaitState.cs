using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningWaitState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningWaitState(
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
            this.Logger.LogDebug($"Inverter {this.InverterStatus.SystemIndex} Positioning Wait State");
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
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.data.WaitContinue)
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
                    else if (this.data.IsWeightMeasure && !this.data.IsWeightMeasureDone)
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningMeasureStartMovingState(
                                this.data,
                                this.ParentStateMachine,
                                this.Inverter,
                                this.Logger));
                    }
                    else if (this.data.IsProfileCalibrate && !this.data.IsProfileCalibrateDone)
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningProfileStartMovingState(
                                this.ParentStateMachine,
                                this.data,
                                this.Inverter,
                                this.Logger));
                    }
                    else if (this.data.IsHorizontalCalibrate || this.data.IsBayCalibrate)
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningHorizontalCalibrateStartMovingState(
                                this.ParentStateMachine,
                                this.data,
                                this.Inverter,
                                this.Logger));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(
                            new PositioningStartMovingState(
                                this.ParentStateMachine,
                                this.data,
                                this.Inverter,
                                this.Logger));
                    }
                }
                else
                {
                    this.Logger.LogTrace("Waiting for Continue Command");
                }
            }
            return returnValue;
        }

        #endregion
    }
}
