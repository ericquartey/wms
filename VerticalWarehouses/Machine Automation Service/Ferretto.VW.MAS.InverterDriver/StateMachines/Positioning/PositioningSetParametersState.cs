using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly VerticalManualMovementsProcedure verticalParams;

        #endregion

        #region Constructors

        public PositioningSetParametersState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
            this.verticalParams = this.ParentStateMachine
                .GetRequiredService<ISetupProceduresDataProvider>()
                .GetVerticalManualMovements(); // NO! these are not manual movements. This needs to be removed!!!!!!!!!!!

            logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPosition, this.data.TargetPosition));
            this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
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
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:ID Parametro={message.ParameterId}");

                switch (message.ParameterId)
                {
                    case InverterParameterId.PositionTargetPosition:
                        if (this.data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical
                            && false    // TODO remove this condition to send brake release/activate parameters
                            )
                        {
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BrakeReleaseTime, (short)this.verticalParams.BrakeReleaseTime));
                            this.Logger.LogDebug($"Set Brake Release Time: {(int)this.verticalParams.BrakeReleaseTime}");
                        }
                        else
                        {
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeed, this.data.TargetSpeed[0]));
                            this.Logger.LogDebug($"Set target Speed: {this.data.TargetSpeed[0]}");
                        }
                        break;

                    case InverterParameterId.BrakeReleaseTime:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BrakeActivatePercent, (int)this.verticalParams.BrakeActivatePercent));
                        this.Logger.LogDebug($"Set Brake Activate Percent: {(int)this.verticalParams.BrakeActivatePercent}");
                        break;

                    case InverterParameterId.BrakeActivatePercent:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeed, this.data.TargetSpeed[0]));
                        this.Logger.LogDebug($"Set target Speed: {this.data.TargetSpeed[0]}");
                        break;

                    case InverterParameterId.PositionTargetSpeed:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionAcceleration, this.data.TargetAcceleration[0]));
                        this.Logger.LogDebug($"Set Acceleration: {this.data.TargetAcceleration[0]}");
                        break;

                    case InverterParameterId.PositionAcceleration:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionDeceleration, this.data.TargetDeceleration[0]));
                        this.Logger.LogDebug($"Set Deceleration: {this.data.TargetDeceleration[0]}");
                        break;

                    case InverterParameterId.PositionDeceleration:
                        this.ParentStateMachine.ChangeState(
                            new PositioningEnableOperationState(
                                this.ParentStateMachine,
                                this.data,
                                this.InverterStatus as IPositioningInverterStatus,
                                this.Logger));
                        break;
                }
            }
            return returnValue;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
