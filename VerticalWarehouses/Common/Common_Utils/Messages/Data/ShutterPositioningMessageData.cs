using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ShutterPositioningMessageData : IShutterPositioningMessageData
    {
        #region Constructors

        public ShutterPositioningMessageData()
        {
        }

        public ShutterPositioningMessageData(
            ShutterPosition shutterPosition,
            ShutterMovementDirection shutterMovementDirection,
            ShutterType shutterType,
            int bayNumber,
            decimal speedRate,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.BayNumber = bayNumber;
            this.SpeedRate = speedRate;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(IShutterPositioningMessageData shutterpositioningMessageData)
        {
            if (shutterpositioningMessageData == null)
            {
                throw new System.ArgumentNullException(nameof(shutterpositioningMessageData));
            }

            this.ShutterPosition = shutterpositioningMessageData.ShutterPosition;
            this.ShutterMovementDirection = shutterpositioningMessageData.ShutterMovementDirection;
            this.ShutterType = shutterpositioningMessageData.ShutterType;
            this.SpeedRate = shutterpositioningMessageData.SpeedRate;
            this.Verbosity = shutterpositioningMessageData.Verbosity;
        }

        #endregion

        #region Properties

        public int BayNumber { get; set; }

        public ShutterMovementDirection ShutterMovementDirection { get; set; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; set; }

        public decimal SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
