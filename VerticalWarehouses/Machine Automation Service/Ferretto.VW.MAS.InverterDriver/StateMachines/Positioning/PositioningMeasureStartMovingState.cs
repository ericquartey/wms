using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningMeasureStartMovingState : PositioningStartMovingState
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IElevatorDataProvider elevatorProvider;

        private readonly IErrorsProvider errorProvider;

        private readonly ElevatorAxis verticalParams;

        private int retryCount;

        private DateTime? startTime;

        #endregion

        #region Constructors

        public PositioningMeasureStartMovingState(
            IInverterPositioningFieldMessageData data,
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, data, inverterStatus, logger)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.elevatorProvider = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>();
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.verticalParams = this.elevatorProvider.GetAxis(Orientation.Vertical);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Inverter.PositionControlWord.ImmediateChangeSet = true;
            base.Start();

            this.Logger.LogInformation("Starting measure of weight.");
            this.startTime = null;
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

            if (this.startTime.HasValue)
            {
                if ((DateTime.UtcNow - this.startTime.Value).TotalMilliseconds > this.verticalParams.WeightMeasurement.MeasureTime * 100)
                {
                    this.RequestSample();
                    this.startTime = DateTime.UtcNow;
                }
            }
            else
            {
                this.startTime = DateTime.UtcNow;
            }

            if (message.ParameterId == InverterParameterId.TorqueCurrent)
            {
                var current = message.UShortPayload / 10.0;

                if (current == 0)
                {
                    if (this.retryCount > 3)
                    {
                        this.Logger.LogError($"PositioningMeasureStartMovingState zero TorqueCurrent");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    }
                    else
                    {
                        this.Logger.LogDebug($"Received zero TorqueCurrent, retry {this.retryCount}");
                        this.RequestSample();
                    }
                    return true;
                }
                this.data.MeasuredWeight = (Math.Pow(current, 2) * this.verticalParams.WeightMeasurement.MeasureConst2)
                    + (current * this.verticalParams.WeightMeasurement.MeasureConst1)
                    + this.verticalParams.WeightMeasurement.MeasureConst0;
                this.data.AbsorbedCurrent = current;

                this.Logger.LogInformation($"Weight measured {this.data.MeasuredWeight:0.00}. Current {current}. k2 {this.verticalParams.WeightMeasurement.MeasureConst2}. k1 {this.verticalParams.WeightMeasurement.MeasureConst1}. k0 {this.verticalParams.WeightMeasurement.MeasureConst0}");
                this.data.IsWeightMeasureDone = true;

                if (this.data.LoadingUnitId.HasValue)
                {
                    try
                    {
                        this.ParentStateMachine
                            .GetRequiredService<ILoadingUnitsDataProvider>()
                            .SetWeight(this.data.LoadingUnitId.Value, this.data.MeasuredWeight);

                        this.ScaleMovementsByWeight();

                        var notificationMessage = new FieldNotificationMessage(
                            this.data,
                            $"Positioning weight measured",
                            FieldMessageActor.DeviceManager,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.Positioning,
                            MessageStatus.OperationUpdateData,
                            this.InverterStatus.SystemIndex);

                        this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
                    }
                    catch
                    {
                        this.errorProvider.RecordNew(MachineErrorCode.LoadUnitWeightExceeded);
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
            this.retryCount++;
            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.TorqueCurrent));
        }

        private void ScaleMovementsByWeight()
        {
            var invertersProvider = this.ParentStateMachine.GetRequiredService<IInvertersProvider>();

            try
            {
                var movementParameters = this.elevatorProvider.ScaleMovementsByWeight(Orientation.Vertical, true);

                var targetPosition = this.data.TargetPositionOriginal; // invertersProvider.ConvertPulsesToMillimeters(this.data.TargetPosition, Orientation.Vertical);
                //targetPosition += axis.Offset;

                var positioningData = new PositioningFieldMessageData(
                    Axis.Vertical,
                    MovementType.Absolute,
                    targetPosition,
                    new[] { movementParameters.Speed * this.data.FeedRate },
                    new[] { movementParameters.Acceleration },
                    new[] { movementParameters.Deceleration },
                    numberCycles: 0,
                    waitContinue: false,
                    computeElongation: true,
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

                this.Logger.LogDebug($"ScaleMovementsByWeight: {MovementMode.PositionAndMeasureWeight}; " +
                    $"targetPosition: {this.data.TargetPosition}; " +
                    $"speed: {this.data.TargetSpeed[0]}; " +
                    $"acceleration: {this.data.TargetAcceleration[0]}; " +
                    $"deceleration: {this.data.TargetDeceleration[0]} ");
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex.Message);
            }
        }

        #endregion
    }
}
