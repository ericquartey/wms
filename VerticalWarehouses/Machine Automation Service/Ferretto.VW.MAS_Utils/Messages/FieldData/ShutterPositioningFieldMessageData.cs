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

        public ShutterPositioningFieldMessageData( IShutterPositioningMessageData shutterpositioningMessageData)
        {           
            this.ShutterPosition = shutterpositioningMessageData.ShutterPosition;
            this.ShutterMovementDirection = shutterpositioningMessageData.ShutterMovementDirection;
            this.ShutterType = shutterpositioningMessageData.ShutterType;
            this.SpeedRate = shutterpositioningMessageData.SpeedRate;
            this.Verbosity = shutterpositioningMessageData.Verbosity;
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
