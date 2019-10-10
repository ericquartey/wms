using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
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
            double speedRate,
            double higherDistance,
            double lowerDistance,
            MovementMode movementMode,
            MovementType movementType,
            int requestedCycles,
            int delay,
            double highSpeedDurationOpen,
            double highSpeedDurationClose,
            double lowerSpeed,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
            this.HigherDistance = higherDistance;
            this.LowerDistance = lowerDistance;
            this.MovementMode = movementMode;
            this.MovementType = movementType;
            this.RequestedCycles = requestedCycles;
            this.Delay = delay;
            this.HighSpeedDurationOpen = highSpeedDurationOpen;
            this.HighSpeedDurationClose = highSpeedDurationClose;
            this.LowerSpeed = lowerSpeed;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(IShutterPositioningMessageData shutterPositioningMessageData)
        {
            if (shutterPositioningMessageData == null)
            {
                throw new System.ArgumentNullException(nameof(shutterPositioningMessageData));
            }

            this.ShutterPosition = shutterPositioningMessageData.ShutterPosition;
            this.ShutterMovementDirection = shutterPositioningMessageData.ShutterMovementDirection;
            this.ShutterType = shutterPositioningMessageData.ShutterType;
            this.SpeedRate = shutterPositioningMessageData.SpeedRate;
            this.HigherDistance = shutterPositioningMessageData.HigherDistance;
            this.LowerDistance = shutterPositioningMessageData.LowerDistance;
            this.MovementMode = shutterPositioningMessageData.MovementMode;
            this.MovementType = shutterPositioningMessageData.MovementType;
            this.RequestedCycles = shutterPositioningMessageData.RequestedCycles;
            this.Delay = shutterPositioningMessageData.Delay;
            this.HighSpeedDurationOpen = shutterPositioningMessageData.HighSpeedDurationOpen;
            this.HighSpeedDurationClose = shutterPositioningMessageData.HighSpeedDurationClose;
            this.LowerSpeed = shutterPositioningMessageData.LowerSpeed;
            this.Verbosity = shutterPositioningMessageData.Verbosity;
            this.ExecutedCycles = shutterPositioningMessageData.ExecutedCycles;
        }

        #endregion

        #region Properties

        public int Delay { get; set; }

        public int ExecutedCycles { get; set; }

        public double HigherDistance { get; }

        public double HighSpeedDurationClose { get; }

        public double HighSpeedDurationOpen { get; }

        public double LowerDistance { get; }

        public double LowerSpeed { get; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; }

        public int RequestedCycles { get; set; }

        public ShutterMovementDirection ShutterMovementDirection { get; set; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; set; }

        public double SpeedRate { get; set; }

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
