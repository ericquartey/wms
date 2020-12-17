using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterShutterPositioningFieldMessageData : FieldMessageData, IInverterShutterPositioningFieldMessageData
    {
        #region Constructors

        public InverterShutterPositioningFieldMessageData(
            IShutterPositioningFieldMessageData shutterPositioningFieldMessageData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (shutterPositioningFieldMessageData is null)
            {
                throw new System.ArgumentNullException(nameof(shutterPositioningFieldMessageData));
            }

            this.ShutterPosition = shutterPositioningFieldMessageData.ShutterPosition;
            this.ShutterType = shutterPositioningFieldMessageData.ShutterType;
            this.ShutterMovementDirection = shutterPositioningFieldMessageData.ShutterMovementDirection;
            this.SpeedRate = (int)shutterPositioningFieldMessageData.SpeedRate;
            this.MovementType = shutterPositioningFieldMessageData.MovementType;
            this.HighSpeedDurationOpen = (int)shutterPositioningFieldMessageData.HighSpeedDurationOpen;
            this.HighSpeedDurationClose = (int)shutterPositioningFieldMessageData.HighSpeedDurationClose;
            this.HighSpeedHalfDurationOpen = (int?)shutterPositioningFieldMessageData.HighSpeedHalfDurationOpen;
            this.HighSpeedHalfDurationClose = (int?)shutterPositioningFieldMessageData.HighSpeedHalfDurationClose;
            this.LowerSpeed = (int)shutterPositioningFieldMessageData.LowerSpeed;
            this.WaitContinue = shutterPositioningFieldMessageData.WaitContinue;
        }

        #endregion

        #region Properties

        public int HighSpeedDurationClose { get; set; }

        public int HighSpeedDurationOpen { get; set; }

        public int? HighSpeedHalfDurationClose { get; set; }

        public int? HighSpeedHalfDurationOpen { get; set; }

        public int LowerSpeed { get; set; }

        public short MovementDuration { get; set; }

        public MovementType MovementType { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; }

        public int SpeedRate { get; set; }

        public bool WaitContinue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Direction:{this.ShutterMovementDirection.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
