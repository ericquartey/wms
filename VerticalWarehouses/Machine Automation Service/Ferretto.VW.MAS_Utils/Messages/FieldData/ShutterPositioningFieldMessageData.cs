using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositioningFieldMessageData : IShutterPositioningFieldMessageData
    {
        #region Constructors

        public ShutterPositioningFieldMessageData(ShutterPosition shutterPosition, ShutterMovementDirection shutterMovementDirection, ShutterType shutterType, byte systemIndex,
           decimal speed, decimal acceleration, decimal deceleration, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
            this.ShutterPosition = shutterPosition;
            this.ShutterPositionMovement = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SystemIndex = systemIndex;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public ShutterType ShutterType { get; }

        public ShutterMovementDirection ShutterPositionMovement { get; }

        public byte SystemIndex { get; set; }

        public decimal TargetAcceleration { get; set; }

        public decimal TargetDeceleration { get; set; }

        public decimal TargetSpeed { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
