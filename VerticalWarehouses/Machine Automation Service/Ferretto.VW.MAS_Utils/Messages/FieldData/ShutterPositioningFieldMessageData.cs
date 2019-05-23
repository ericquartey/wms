using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositioningFieldMessageData : IShutterPositioningFieldMessageData
    {
        #region Constructors

        public ShutterPositioningFieldMessageData(ShutterPosition shutterPosition, ShutterMovementDirection shutterMovementDirection, byte systemIndex, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;

            this.ShutterPosition = shutterPosition;

            this.ShutterPositionMovement = shutterMovementDirection;

            this.SystemIndex = systemIndex;
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public ShutterMovementDirection ShutterPositionMovement { get; }

        public byte SystemIndex { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
