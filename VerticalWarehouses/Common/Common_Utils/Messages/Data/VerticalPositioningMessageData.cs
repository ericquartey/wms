using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class VerticalPositioningMessageData : IVerticalPositioningMessageData
    {
        #region Constructors

        public VerticalPositioningMessageData(Axis axisMovement, MovementType movementType, decimal target, decimal speed, decimal acceleration,
            decimal deceleration, int numberCycles, decimal lowerBound, decimal upperBound,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
            this.NumberCycles = numberCycles;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            //this.Resolution = resolution;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; private set; }

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public decimal CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public decimal LowerBound { get; }

        public MovementType MovementType { get; private set; }

        public int NumberCycles { get; }

        public decimal Resolution { get; }

        public decimal TargetAcceleration { get; private set; }

        public decimal TargetDeceleration { get; private set; }

        public decimal TargetPosition { get; private set; }

        public decimal TargetSpeed { get; private set; }

        public decimal UpperBound { get; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
