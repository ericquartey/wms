using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
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
            decimal target,
            decimal speed,
            decimal acceleration,
            decimal deceleration,
            int numberCycles,
            decimal lowerBound,
            decimal upperBound,
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
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public decimal CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public decimal LowerBound { get; set; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; set; }

        public decimal TargetAcceleration { get; set; }

        public decimal TargetDeceleration { get; set; }

        public decimal TargetPosition { get; set; }

        public decimal TargetSpeed { get; set; }

        public decimal UpperBound { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisMovement:{this.AxisMovement.ToString()} MovementType:{this.MovementType.ToString()} TargetPosition:{this.TargetPosition} TargetSpeed:{this.TargetSpeed} TargetAcceleration:{this.TargetAcceleration} TargetDeceleration:{this.TargetDeceleration} NumberCycles:{this.NumberCycles} LowerBound:{this.LowerBound} UpperBound:{this.UpperBound}";
        }

        #endregion
    }
}
