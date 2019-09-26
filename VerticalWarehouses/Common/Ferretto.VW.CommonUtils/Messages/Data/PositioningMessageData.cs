using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class DataSample
    {
        #region Properties

        public System.DateTime TimeStamp { get; set; }

        public double Value { get; set; }

        #endregion
    }

    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Check if we really need this constructor.")]
        public PositioningMessageData(
            Axis axisMovement,
            MovementType movementType,
            MovementMode movementMode,
            double target,
            double[] speed,
            double[] acceleration,
            double[] deceleration,
            int numberCycles,
            double lowerBound,
            double upperBound,
            int delay,
            double[] switchPosition,
            HorizontalMovementDirection direction,
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
            this.NumberCycles = numberCycles;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.SwitchPosition = switchPosition;
            this.Delay = delay;
            this.Direction = direction;
        }

        public PositioningMessageData(IPositioningMessageData other)
        {
            this.AxisMovement = other.AxisMovement;
            this.MovementType = other.MovementType;
            this.MovementMode = other.MovementMode;
            this.TargetPosition = other.TargetPosition;
            this.TargetSpeed = other.TargetSpeed;
            this.TargetAcceleration = other.TargetAcceleration;
            this.TargetDeceleration = other.TargetDeceleration;
            this.NumberCycles = other.NumberCycles;
            this.LowerBound = other.LowerBound;
            this.UpperBound = other.UpperBound;
            this.SwitchPosition = other.SwitchPosition;
            this.Delay = other.Delay;
            this.Direction = other.Direction;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public double? CurrentPosition { get; set; }

        public int Delay { get; set; }

        public HorizontalMovementDirection Direction { get; set; }

        public int ExecutedCycles { get; set; }

        public bool IsOneKMachine { get; set; }

        public bool IsStartedOnBoard { get; set; }

        public double? LoadedNetWeight { get; set; }

        public int? LoadingUnitId { get; set; }

        public double LowerBound { get; set; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; set; }

        public double[] SwitchPosition { get; set; }

        public double[] TargetAcceleration { get; set; }

        public double[] TargetDeceleration { get; set; }

        public double TargetPosition { get; set; }

        public double[] TargetSpeed { get; set; }

        public DataSample TorqueCurrentSample { get; set; }

        public double UpperBound { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisMovement:{this.AxisMovement.ToString()} MovementType:{this.MovementType.ToString()} TargetPosition:{this.TargetPosition} TargetSpeed:{this.TargetSpeed} TargetAcceleration:{this.TargetAcceleration} TargetDeceleration:{this.TargetDeceleration} NumberCycles:{this.NumberCycles} LowerBound:{this.LowerBound} UpperBound:{this.UpperBound} Delay:{this.Delay}";
        }

        #endregion
    }
}
