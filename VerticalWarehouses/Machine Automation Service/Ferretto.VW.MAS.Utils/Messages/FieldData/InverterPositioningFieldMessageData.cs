using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterPositioningFieldMessageData : FieldMessageData, IInverterPositioningFieldMessageData
    {
        #region Constructors

        public InverterPositioningFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        public InverterPositioningFieldMessageData(
            IPositioningFieldMessageData positioningFieldMessageData,
            int[] targetAcceleration,
            int[] targetDeceleration,
            int startPosition,
            int targetPosition,
            double targetPositionOriginal,
            int[] targetSpeed,
            int[] switchPosition,
            int direction,
            bool refreshAll,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (positioningFieldMessageData is null)
            {
                throw new System.ArgumentNullException(nameof(positioningFieldMessageData));
            }

            this.AxisMovement = positioningFieldMessageData.AxisMovement;
            this.MovementType = positioningFieldMessageData.MovementType;
            this.NumberCycles = positioningFieldMessageData.NumberCycles;
            this.TargetAcceleration = targetAcceleration;
            this.TargetDeceleration = targetDeceleration;
            this.StartPosition = startPosition;
            this.TargetPosition = targetPosition;
            this.TargetPositionOriginal = targetPositionOriginal;
            this.TargetSpeed = targetSpeed;
            this.SwitchPosition = switchPosition;
            this.RefreshAll = refreshAll;
            this.Direction = direction;
            this.WaitContinue = positioningFieldMessageData.WaitContinue;
            this.RequestingBay = positioningFieldMessageData.RequestingBay;

            this.IsTorqueCurrentSamplingEnabled = positioningFieldMessageData.IsTorqueCurrentSamplingEnabled;
            this.IsWeightMeasure = positioningFieldMessageData.IsWeightMeasure;
            this.LoadedNetWeight = positioningFieldMessageData.LoadedNetWeight;
            this.LoadingUnitId = positioningFieldMessageData.LoadingUnitId;
            this.FeedRate = positioningFieldMessageData.FeedRate;
            this.ComputeElongation = positioningFieldMessageData.ComputeElongation;
            this.IsProfileCalibrate = positioningFieldMessageData.IsProfileCalibrate;
            this.IsHorizontalCalibrate = positioningFieldMessageData.IsHorizontalCalibrate;
            this.IsPickupMission = positioningFieldMessageData.IsPickupMission;

            this.IsWeightMeasureDone = false;
            this.MeasuredWeight = 0.0;
            this.IsProfileCalibrateDone = false;
            this.AbsorbedCurrent = 0.0;
        }

        #endregion

        #region Properties

        public double AbsorbedCurrent { get; set; }

        public Axis AxisMovement { get; set; }

        public bool ComputeElongation { get; set; }

        public int Direction { get; set; }

        public double FeedRate { get; set; }

        public bool IsHorizontalCalibrate { get; set; }

        public bool IsPickupMission { get; set; }

        public bool IsProfileCalibrate { get; set; }

        public bool IsProfileCalibrateDone { get; set; }

        public bool IsTorqueCurrentSamplingEnabled { get; set; }

        public bool IsWeightMeasure { get; set; }

        public bool IsWeightMeasureDone { get; set; }

        public double? LoadedNetWeight { get; }

        public int? LoadingUnitId { get; set; }

        public double MeasuredWeight { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public bool RefreshAll { get; }

        public BayNumber RequestingBay { get; set; }

        public int StartPosition { get; set; }

        public int[] SwitchPosition { get; set; }

        public int[] TargetAcceleration { get; set; }

        public int[] TargetDeceleration { get; set; }

        public int TargetPosition { get; set; }

        public double TargetPositionOriginal { get; set; }

        public int[] TargetSpeed { get; set; }

        public bool WaitContinue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Axis:{this.AxisMovement.ToString()} Type:{this.MovementType.ToString()} NumberCycles:{this.NumberCycles} Acceleration:{this.TargetAcceleration} Deceleration:{this.TargetDeceleration} Position:{this.TargetPosition} Speed:{this.TargetSpeed}";
        }

        #endregion
    }
}
