using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ShutterPositioningMessageData : IShutterPositioningMessageData
    {

        #region Constructors

        public ShutterPositioningMessageData(ShutterPosition shutterPosition, ShutterMovementDirection shutterMovementDirection, ShutterType shutterType, byte systemIndex, int bayNumber,
            decimal speedRate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SystemIndex = systemIndex;
            this.BayNumber = bayNumber;            
            this.SpeedRate = speedRate;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(IShutterPositioningMessageData shutterpositioningMessageData)
        {
            this.ShutterPosition = shutterpositioningMessageData.ShutterPosition;
            this.ShutterMovementDirection = shutterpositioningMessageData.ShutterMovementDirection;
            this.ShutterType = shutterpositioningMessageData.ShutterType;
            this.SystemIndex = shutterpositioningMessageData.SystemIndex;
            this.SpeedRate = shutterpositioningMessageData.SpeedRate;
            this.Verbosity = shutterpositioningMessageData.Verbosity;
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterType ShutterType { get; }

        public byte SystemIndex { get; set; }

        public int BayNumber { get; }

        public decimal SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
