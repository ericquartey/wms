using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningMeasureStartMovingState : PositioningStartMovingState
    {
        #region Fields

        // TODO transform these constants in configuration parameters?
        private const double MinimumLoadOnBoard = 10.0;

        private const int WeightTolerance = 50;

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IElevatorDataProvider elevatorProvider;

        private readonly ElevatorAxis verticalParams;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PositioningMeasureStartMovingState(
            IInverterPositioningFieldMessageData data,
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.data = data;

            this.elevatorProvider = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>();
            this.verticalParams = this.elevatorProvider.GetAxis(Orientation.Vertical);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Inverter.PositionControlWord.ImmediateChangeSet = true;
            base.Start();

            this.Logger.LogInformation("Starting measure of weight.");
            this.startTime = DateTime.MinValue;
        }

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

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            base.ValidateCommandResponse(message);

            if (this.startTime == DateTime.MinValue)
            {
                this.startTime = DateTime.UtcNow;
            }
            else
            {
                if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.verticalParams.WeightMeasureTime * 100)
                {
                    this.RequestSample();
                    this.startTime = DateTime.UtcNow;
                }
            }
            if (message.ParameterId == InverterParameterId.TorqueCurrent)
            {
                this.data.MeasuredWeight = (message.UShortPayload * this.verticalParams.WeightMeasureMultiply / 10.0) + this.verticalParams.WeightMeasureSum;
                this.Logger.LogInformation($"Weight measured {this.data.MeasuredWeight}. Current {message.UShortPayload / 10.0}. kMul {this.verticalParams.WeightMeasureMultiply}. kSum {this.verticalParams.WeightMeasureSum}");
                this.data.IsWeightMeasureDone = true;
                if (this.data.LoadingUnitId.HasValue
                    && this.data.MeasuredWeight > MinimumLoadOnBoard
                    && this.data.MeasuredWeight < this.elevatorProvider.GetStructuralProperties().MaximumLoadOnBoard)
                {
                    this.ParentStateMachine.GetRequiredService<ILoadingUnitsProvider>().SetWeight(this.data.LoadingUnitId.Value, this.data.MeasuredWeight);
                    if (!this.data.LoadedNetWeight.HasValue
                        || Math.Abs(this.data.MeasuredWeight - this.data.LoadedNetWeight.Value) > WeightTolerance
                        )
                    {
                        this.ScaleMovementsByWeight();
                    }
                }

                this.ParentStateMachine.ChangeState(
                    new PositioningMeasureDisableOperationState(
                        this.ParentStateMachine,
                        this.data,
                        this.Inverter,
                        this.Logger));
            }

            return true;
        }

        private void RequestSample()
        {
            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.TorqueCurrent));
        }

        private void ScaleMovementsByWeight()
        {
            var invertersProvider = this.ParentStateMachine.GetRequiredService<IInvertersProvider>();
            var axis = this.elevatorProvider.GetVerticalAxis();
            MovementParameters movementParameters = null;
            try
            {
                movementParameters = this.elevatorProvider.ScaleMovementsByWeight(Orientation.Vertical);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex.Message);
            }
            if (movementParameters != null)
            {
                var targetPosition = invertersProvider.ConvertPulsesToMillimeters(this.data.TargetPosition, Orientation.Vertical);
                targetPosition += axis.Offset;

                var positioningData = new PositioningFieldMessageData(
                    Axis.Vertical,
                    MovementType.Absolute,
                    targetPosition,
                    new[] { movementParameters.Speed * this.data.FeedRate },
                    new[] { movementParameters.Acceleration },
                    new[] { movementParameters.Deceleration },
                    0,
                    false,
                    BayNumber.ElevatorBay);

                var currentPosition = 0;
                if (this.Inverter is AngInverterStatus angInverter)
                {
                    currentPosition = angInverter.CurrentPositionAxisVertical;
                }
                else if (this.Inverter is AcuInverterStatus acuInverter)
                {
                    currentPosition = acuInverter.CurrentPosition;
                }
                positioningData.SwitchPosition = new[] { 0.0 };
                invertersProvider.ComputePositioningValues(this.Inverter, positioningData, Orientation.Vertical, currentPosition, false, out var fieldMessageData);
                this.data.TargetPosition = fieldMessageData.TargetPosition;
                this.data.TargetSpeed = fieldMessageData.TargetSpeed;
                this.data.TargetAcceleration = fieldMessageData.TargetAcceleration;
                this.data.TargetDeceleration = fieldMessageData.TargetDeceleration;

                this.Logger.LogDebug($"ScaleMovementsByWeight: {MovementMode.PositionAndMeasure}; " +
                    $"targetPosition: {this.data.TargetPosition}; " +
                    $"speed: {this.data.TargetSpeed[0]}; " +
                    $"acceleration: {this.data.TargetAcceleration[0]}; " +
                    $"deceleration: {this.data.TargetDeceleration[0]} ");
            }
        }

        #endregion
    }
}
