using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class PositioningFieldMessageData : IPositioningFieldMessageData
    {
        #region Constructors

        public PositioningFieldMessageData(Axis axisMovement, MovementType movementType, decimal target, decimal speed, decimal acceleration, decimal deceleration, int numberCycles, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
            this.NumberCycles = numberCycles;
        }

        public PositioningFieldMessageData(IPositioningMessageData verticalPositioningMessageData)
        {
            this.AxisMovement = verticalPositioningMessageData.AxisMovement;
            this.MovementType = verticalPositioningMessageData.MovementType;
            this.Resolution = verticalPositioningMessageData.Resolution;
            this.TargetPosition = verticalPositioningMessageData.TargetPosition;
            this.TargetSpeed = verticalPositioningMessageData.TargetSpeed;
            this.TargetAcceleration = verticalPositioningMessageData.TargetAcceleration;
            this.TargetDeceleration = verticalPositioningMessageData.TargetDeceleration;
            this.Verbosity = verticalPositioningMessageData.Verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public decimal Resolution { get; }

        public decimal TargetAcceleration { get; set; }

        public decimal TargetDeceleration { get; set; }

        public decimal TargetPosition { get; set; }

        public decimal TargetSpeed { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
