using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositioningFieldMessageData : IShutterPositioningFieldMessageData
    {
        #region Constructors

        public ShutterPositioningFieldMessageData(ShutterPosition shutterPosition, ShutterMovementDirection shutterMovementDirection, ShutterType shutterType,
           decimal speedRate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
        }

        public ShutterPositioningFieldMessageData( IShutterPositioningMessageData shutterPositioningMessageData)
        {           
            this.ShutterPosition = shutterPositioningMessageData.ShutterPosition;
            this.ShutterMovementDirection = shutterPositioningMessageData.ShutterMovementDirection;
            this.ShutterType = shutterPositioningMessageData.ShutterType;
            this.SpeedRate = shutterPositioningMessageData.SpeedRate;
            this.Verbosity = shutterPositioningMessageData.Verbosity;
        }
        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public ShutterType ShutterType { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public decimal SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
