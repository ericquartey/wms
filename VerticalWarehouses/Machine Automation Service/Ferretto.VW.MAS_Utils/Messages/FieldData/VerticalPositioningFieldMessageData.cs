using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class VerticalPositioningFieldMessageData : IVerticalPositioningFieldMessageData
    {
        #region Constructors

        public VerticalPositioningFieldMessageData(IPositioningFieldMessageData positioningFieldMessageData, decimal accelerationConversion,
            decimal decellerationConversion, decimal resolutionCalibration, decimal speedConversion, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = positioningFieldMessageData.AxisMovement;
            this.MovementType = positioningFieldMessageData.MovementType;
            this.NumberCycles = positioningFieldMessageData.NumberCycles;
            this.TargetAcceleration = decimal.ToInt32(positioningFieldMessageData.TargetAcceleration * accelerationConversion);
            this.TargetDeceleration = decimal.ToInt32(positioningFieldMessageData.TargetDeceleration * decellerationConversion);
            this.TargetPosition = decimal.ToInt32(resolutionCalibration * positioningFieldMessageData.TargetPosition);
            this.TargetSpeed = decimal.ToInt32(positioningFieldMessageData.TargetSpeed * speedConversion);
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
