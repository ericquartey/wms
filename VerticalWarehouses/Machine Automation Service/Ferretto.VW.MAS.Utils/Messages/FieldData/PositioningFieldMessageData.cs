using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class PositioningFieldMessageData : FieldMessageData, IPositioningFieldMessageData
    {
        #region Constructors

        public PositioningFieldMessageData(
            Axis axisMovement,
            MovementType movementType,
            decimal target,
            decimal[] speed,
            decimal[] acceleration,
            decimal[] deceleration,
            int numberCycles,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.NumberCycles = numberCycles;
        }

        public PositioningFieldMessageData(
            IPositioningMessageData messageData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
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
            this.Direction = messageData.Direction;
            this.SwitchPosition = messageData.SwitchPosition;
            this.LoadedGrossWeight = messageData.LoadedGrossWeight;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public HorizontalMovementDirection Direction { get; set; }

        public decimal LoadedGrossWeight { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public decimal[] SwitchPosition { get; set; }

        public decimal[] TargetAcceleration { get; set; }

        public decimal[] TargetDeceleration { get; set; }

        public decimal TargetPosition { get; set; }

        public decimal[] TargetSpeed { get; set; }

        public (decimal Value, System.DateTime TimeStamp) TorqueCurrentSample { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Axis:{this.AxisMovement.ToString()} Type:{this.MovementType.ToString()} NumberCycles:{this.NumberCycles} Acceleration:{this.TargetAcceleration} Deceleration:{this.TargetDeceleration} Position:{this.TargetPosition} Speed:{this.TargetSpeed}";
        }

        #endregion
    }
}
