using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPositioningFieldMessageData : IInverterPositioningFieldMessageData
    {
        #region Constructors

        public InverterPositioningFieldMessageData(
            IPositioningFieldMessageData positioningFieldMessageData,
            int targetAcceleration,
            int targetDeceleration,
            int targetPosition,
            int targetSpeed,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = positioningFieldMessageData.AxisMovement;
            this.MovementType = positioningFieldMessageData.MovementType;
            this.NumberCycles = positioningFieldMessageData.NumberCycles;
            this.TargetAcceleration = targetAcceleration;
            this.TargetDeceleration = targetDeceleration;
            this.TargetPosition = targetPosition;
            this.TargetSpeed = targetSpeed;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public int TargetAcceleration { get; set; }

        public int TargetDeceleration { get; set; }

        public int TargetPosition { get; set; }

        public int TargetSpeed { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
