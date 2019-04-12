using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData(Axis axisMovement, MovementType movementType, decimal target, decimal speed, decimal acceleration, decimal deceleration, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
        }

        public PositioningMessageData(PositioningFieldMessageData data)
        {
            this.AxisMovement = data.AxisMovement;
            this.MovementType = data.MovementType;
            this.TargetPosition = data.TargetPosition;
            this.TargetSpeed = data.TargetSpeed;
            this.TargetAcceleration = data.TargetAcceleration;
            this.TargetDeceleration = data.TargetDeceleration;
            this.Verbosity = data.Verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; private set; }

        public MovementType MovementType { get; private set; }

        public decimal TargetAcceleration { get; private set; }

        public decimal TargetDeceleration { get; private set; }

        public decimal TargetPosition { get; private set; }

        public decimal TargetSpeed { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
