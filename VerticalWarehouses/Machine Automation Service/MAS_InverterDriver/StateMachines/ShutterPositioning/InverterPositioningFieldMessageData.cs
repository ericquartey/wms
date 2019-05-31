using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class InverterPositioningFieldMessageData : IInverterPositioningFieldMessageData
    {
        #region Constructors

        public InverterPositioningFieldMessageData(IPositioningFieldMessageData positioningFieldMessageData, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = positioningFieldMessageData.AxisMovement;
            this.MovementType = positioningFieldMessageData.MovementType;
            this.NumberCycles = positioningFieldMessageData.NumberCycles;
            this.TargetAcceleration = decimal.ToInt32(positioningFieldMessageData.TargetAcceleration * positioningFieldMessageData.Resolution);
            this.TargetDeceleration = decimal.ToInt32(positioningFieldMessageData.TargetDeceleration * positioningFieldMessageData.Resolution);
            this.TargetPosition = decimal.ToInt32(positioningFieldMessageData.TargetPosition * positioningFieldMessageData.Resolution);
            this.TargetSpeed = decimal.ToInt32(positioningFieldMessageData.TargetSpeed * positioningFieldMessageData.Resolution);
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
