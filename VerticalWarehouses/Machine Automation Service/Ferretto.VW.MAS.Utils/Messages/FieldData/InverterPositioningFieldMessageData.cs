using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPositioningFieldMessageData : FieldMessageData, IInverterPositioningFieldMessageData
    {
        #region Constructors

        public InverterPositioningFieldMessageData(
            IPositioningFieldMessageData positioningFieldMessageData,
            int[] targetAcceleration,
            int[] targetDeceleration,
            int targetPosition,
            int[] targetSpeed,
            int[] switchPosition,
            int direction,
            bool refreshAll,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisMovement = positioningFieldMessageData.AxisMovement;
            this.MovementType = positioningFieldMessageData.MovementType;
            this.NumberCycles = positioningFieldMessageData.NumberCycles;
            this.TargetAcceleration = targetAcceleration;
            this.TargetDeceleration = targetDeceleration;
            this.TargetPosition = targetPosition;
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

            this.IsWeightMeasureDone = false;
            this.MeasuredWeight = 0.0;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public int Direction { get; set; }

        public double FeedRate { get; set; }

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

        public int[] SwitchPosition { get; set; }

        public int[] TargetAcceleration { get; set; }

        public int[] TargetDeceleration { get; set; }

        public int TargetPosition { get; set; }

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
