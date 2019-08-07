using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
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

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Position:{this.ShutterPosition.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
