using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData()
        {
        }

        public PositioningMessageData(
            Axis axisMovement,
            MovementType movementType,
            MovementMode movementMode,
            double target,
            double[] speed,
            double[] acceleration,
            double[] deceleration,
            int requiredCycles,
            double lowerBound,
            double upperBound,
            int delay,
            double[] switchPosition,
            HorizontalMovementDirection direction,
            bool waitContinue = false,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.MovementMode = movementMode;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
            this.RequiredCycles = requiredCycles;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.SwitchPosition = switchPosition;
            this.DelayEnd = delay;
            this.Direction = direction;
            this.WaitContinue = waitContinue;
        }

        public PositioningMessageData(
            Axis axisMovement,
            MovementType movementType,
            MovementMode movementMode,
            double target,
            double[] speed,
            double[] acceleration,
            double[] deceleration,
            double[] switchPosition,
            HorizontalMovementDirection direction,
            bool waitContinue = false,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : this(
                  axisMovement,
                  movementType,
                  movementMode,
                  target,
                  speed,
                  acceleration,
                  deceleration,
                  0,
                  0,
                  0,
                  0,
                  switchPosition,
                  direction,
                  waitContinue,
                  verbosity)
        {
        }

        public PositioningMessageData(IPositioningMessageData other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            this.AxisMovement = other.AxisMovement;
            this.MovementType = other.MovementType;
            this.MovementMode = other.MovementMode;
            this.TargetPosition = other.TargetPosition;
            this.TargetSpeed = other.TargetSpeed;
            this.TargetAcceleration = other.TargetAcceleration;
            this.TargetDeceleration = other.TargetDeceleration;
            this.RequiredCycles = other.RequiredCycles;
            this.LowerBound = other.LowerBound;
            this.UpperBound = other.UpperBound;
            this.SwitchPosition = other.SwitchPosition;
            this.DelayEnd = other.DelayEnd;
            this.DelayStart = other.DelayStart;
            this.Direction = other.Direction;
            this.WaitContinue = other.WaitContinue;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public bool BypassConditions { get; set; } = false;

        public bool ComputeElongation { get; set; }

        public int DelayEnd { get; set; }

        public int DelayStart { get; set; }

        public HorizontalMovementDirection Direction { get; set; }

        public int ExecutedCycles { get; set; }

        public double FeedRate { get; set; }

        public bool IsOneTonMachine { get; set; }

        public bool IsPickupMission { get; set; }

        public bool IsStartedOnBoard { get; set; }

        public bool IsTestStopped { get; set; }

        public double? LoadedNetWeight { get; set; }

        public int? LoadingUnitId { get; set; }

        public double LowerBound { get; set; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; set; }

        public int RequiredCycles { get; set; }

        public int? SourceBayPositionId { get; set; }

        public int? SourceCellId { get; set; }

        public double[] SwitchPosition { get; set; }

        public double[] TargetAcceleration { get; set; }

        public int? TargetBayPositionId { get; set; }

        public int? TargetCellId { get; set; }

        public double[] TargetDeceleration { get; set; }

        public double TargetPosition { get; set; }

        public double[] TargetSpeed { get; set; }

        public DataSample TorqueCurrentSample { get; set; }

        public double UpperBound { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        public bool WaitContinue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisMovement:{this.AxisMovement.ToString()} MovementType:{this.MovementType.ToString()} TargetPosition:{this.TargetPosition} TargetSpeed:{this.TargetSpeed} TargetAcceleration:{this.TargetAcceleration} TargetDeceleration:{this.TargetDeceleration} NumberCycles:{this.RequiredCycles} LowerBound:{this.LowerBound} UpperBound:{this.UpperBound} Delay:{this.DelayEnd}";
        }

        #endregion
    }
}
