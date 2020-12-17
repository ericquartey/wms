using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ShutterPositioningMessageData : IShutterPositioningMessageData
    {
        #region Fields

        private double speedRate;

        #endregion

        #region Constructors

        public ShutterPositioningMessageData()
        {
        }

        public ShutterPositioningMessageData(
            ShutterPosition shutterPosition,
            ShutterMovementDirection shutterMovementDirection,
            ShutterType shutterType,
            double speedRate,
            MovementMode movementMode,
            MovementType movementType,
            int delay,
            double highSpeedDurationOpen,
            double highSpeedDurationClose,
            double? highSpeedHalfDurationOpen,
            double? highSpeedHalfDurationClose,
            double lowerSpeed,
            bool waitContinue = false,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
            this.MovementMode = movementMode;
            this.MovementType = movementType;
            this.Delay = delay;
            this.HighSpeedDurationOpen = highSpeedDurationOpen;
            this.HighSpeedDurationClose = highSpeedDurationClose;
            this.HighSpeedHalfDurationOpen = highSpeedHalfDurationOpen;
            this.HighSpeedHalfDurationClose = highSpeedHalfDurationClose;
            this.LowerSpeed = lowerSpeed;
            this.WaitContinue = waitContinue;
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
            this.MovementType = shutterPositioningMessageData.MovementType;
            this.Delay = shutterPositioningMessageData.Delay;
            this.HighSpeedDurationOpen = shutterPositioningMessageData.HighSpeedDurationOpen;
            this.HighSpeedDurationClose = shutterPositioningMessageData.HighSpeedDurationClose;
            this.HighSpeedHalfDurationOpen = shutterPositioningMessageData.HighSpeedHalfDurationOpen;
            this.HighSpeedHalfDurationClose = shutterPositioningMessageData.HighSpeedHalfDurationClose;
            this.LowerSpeed = shutterPositioningMessageData.LowerSpeed;
            this.Verbosity = shutterPositioningMessageData.Verbosity;
            this.PerformedCycles = shutterPositioningMessageData.PerformedCycles;
            this.WaitContinue = shutterPositioningMessageData.WaitContinue;
        }

        #endregion

        #region Properties

        public bool BypassConditions { get; set; } = false;

        public int Delay { get; set; }

        public double HighSpeedDurationClose { get; }

        public double HighSpeedDurationOpen { get; }

        public double? HighSpeedHalfDurationClose { get; }

        public double? HighSpeedHalfDurationOpen { get; }

        public double LowerSpeed { get; }

        public MovementMode MovementMode { get; set; }

        public MovementType MovementType { get; }

        public int PerformedCycles { get; set; }

        public ShutterMovementDirection ShutterMovementDirection { get; set; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; set; }

        public double SpeedRate
        {
            get => this.speedRate;
            set
            {
                if (value == 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.speedRate = value;
            }
        }

        public MessageVerbosity Verbosity { get; set; }

        public bool WaitContinue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Position:{this.ShutterPosition.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
