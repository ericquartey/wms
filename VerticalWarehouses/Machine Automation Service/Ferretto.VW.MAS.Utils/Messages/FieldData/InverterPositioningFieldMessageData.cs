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
            this.Direction = (targetPosition > 0) ? 2 : 4;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public int Direction { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public bool RefreshAll { get; }

        public int[] SwitchPosition { get; set; }

        public int[] TargetAcceleration { get; set; }

        public int[] TargetDeceleration { get; set; }

        public int TargetPosition { get; set; }

        public int[] TargetSpeed { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Axis:{this.AxisMovement.ToString()} Type:{this.MovementType.ToString()} NumberCycles:{this.NumberCycles} Acceleration:{this.TargetAcceleration} Deceleration:{this.TargetDeceleration} Position:{this.TargetPosition} Speed:{this.TargetSpeed}";
        }

        #endregion
    }
}
