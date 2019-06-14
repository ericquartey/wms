using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData()
        {
        }

        public PositioningMessageData(Axis axisMovement, MovementType movementType, decimal target, decimal speed, decimal acceleration,
            decimal deceleration, int numberCycles, decimal lowerBound, decimal upperBound, decimal resolution,
            ResolutionCalibrationSteps resolutionCalibrationSteps, MessageVerbosity verbosity = MessageVerbosity.Debug)
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
            this.Resolution = resolution;
            this.ResolutionCalibrationSteps = resolutionCalibrationSteps;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public decimal CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public decimal LowerBound { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; set; }

        public decimal Resolution { get; set; }

        public ResolutionCalibrationSteps ResolutionCalibrationSteps { get; set; }

        public decimal TargetAcceleration { get; set; }

        public decimal TargetDeceleration { get; set; }

        public decimal TargetPosition { get; set; }

        public decimal TargetSpeed { get; set; }

        public decimal UpperBound { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
