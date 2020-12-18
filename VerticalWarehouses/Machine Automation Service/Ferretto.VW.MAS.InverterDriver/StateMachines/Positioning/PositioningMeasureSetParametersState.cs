using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningMeasureSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly ElevatorAxis verticalParams;

        #endregion

        #region Constructors

        public PositioningMeasureSetParametersState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;
            this.verticalParams = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);

            logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            var position = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(this.verticalParams.UpperBound, Orientation.Vertical);
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetPosition, position));
            this.Logger.LogDebug($"Set target position: {position}");
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
                        var speed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(this.verticalParams.WeightMeasurement.MeasureSpeed, Orientation.Vertical);
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeed, speed));
                        this.Logger.LogDebug($"Set target Speed: {speed}");
                        break;

                    case InverterParameterId.PositionTargetSpeed:
                        this.ParentStateMachine.ChangeState(new PositioningEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus as IPositioningInverterStatus, this.Logger));
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
