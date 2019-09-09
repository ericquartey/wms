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
            MovementMode movementMode,
            MovementType movementType,
            int requestedCycles,
            int delay,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.BayNumber = bayNumber;
            this.SpeedRate = speedRate;
            this.MovementMode = movementMode;
            this.MovementType = movementType;
            this.RequestedCycles = requestedCycles;
            this.Delay = delay;
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
            this.MovementMode = shutterpositioningMessageData.MovementMode;
            this.MovementType = shutterpositioningMessageData.MovementType;
            this.RequestedCycles = shutterpositioningMessageData.RequestedCycles;
            this.Delay = shutterpositioningMessageData.Delay;
            this.Verbosity = shutterpositioningMessageData.Verbosity;
        }

        #endregion

        #region Properties

        public int BayNumber { get; set; }

        public int Delay { get; set; }

        public int ExecutedCycles { get; set; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; }

        public int RequestedCycles { get; set; }

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
