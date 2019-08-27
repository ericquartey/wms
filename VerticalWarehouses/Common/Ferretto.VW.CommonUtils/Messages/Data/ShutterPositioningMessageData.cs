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
            decimal speedRate,
            MovementMode movementMode,
            int requestedCycles,
            int delay,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
            this.MovementMode = movementMode;
            this.RequestedCycles = requestedCycles;
            this.Delay = delay;
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
            this.MovementMode = shutterPositioningMessageData.MovementMode;
            this.RequestedCycles = shutterPositioningMessageData.RequestedCycles;
            this.Delay = shutterPositioningMessageData.Delay;
            this.Verbosity = shutterPositioningMessageData.Verbosity;
        }

        #endregion



        #region Properties

        public int Delay { get; set; }

        public int ExecutedCycles { get; set; }

        public MovementMode MovementMode { get; set; }

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
