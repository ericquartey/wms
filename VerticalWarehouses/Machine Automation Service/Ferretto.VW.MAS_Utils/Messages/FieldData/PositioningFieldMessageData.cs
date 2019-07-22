using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class PositioningFieldMessageData : IPositioningFieldMessageData
    {
        #region Constructors

        public PositioningFieldMessageData(
            Axis axisMovement,
            MovementType movementType,
            decimal target,
            decimal speed,
            decimal acceleration,
            decimal deceleration,
            int numberCycles,
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
        }

        public PositioningFieldMessageData(IPositioningMessageData messageData)
        {
            if (messageData == null)
            {
                throw new System.ArgumentNullException(nameof(messageData));
            }

            this.AxisMovement = messageData.AxisMovement;
            this.MovementType = messageData.MovementType;
            this.TargetPosition = messageData.TargetPosition;
            this.TargetSpeed = messageData.TargetSpeed;
            this.TargetAcceleration = messageData.TargetAcceleration;
            this.TargetDeceleration = messageData.TargetDeceleration;
            this.Verbosity = messageData.Verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public decimal TargetAcceleration { get; set; }

        public decimal TargetDeceleration { get; set; }

        public decimal TargetPosition { get; set; }

        public decimal TargetSpeed { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
