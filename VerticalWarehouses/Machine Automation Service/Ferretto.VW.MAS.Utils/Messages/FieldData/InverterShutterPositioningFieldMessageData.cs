using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterShutterPositioningFieldMessageData : FieldMessageData, IInverterShutterPositioningFieldMessageData
    {
        #region Constructors

        public InverterShutterPositioningFieldMessageData(
            IShutterPositioningFieldMessageData shutterPositioningFieldMessageData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.ShutterPosition = shutterPositioningFieldMessageData.ShutterPosition;
            this.ShutterType = shutterPositioningFieldMessageData.ShutterType;
            this.ShutterMovementDirection = shutterPositioningFieldMessageData.ShutterMovementDirection;
            this.SpeedRate = decimal.ToInt32(shutterPositioningFieldMessageData.SpeedRate);
            this.HigherDistance = decimal.ToInt32(shutterPositioningFieldMessageData.HigherDistance);
            this.LowerDistance = decimal.ToInt32(shutterPositioningFieldMessageData.LowerDistance);
            this.MovementType = shutterPositioningFieldMessageData.MovementType;
            this.HighSpeedDurationOpen = decimal.ToInt32(shutterPositioningFieldMessageData.HighSpeedDurationOpen);
            this.HighSpeedDurationClose = decimal.ToInt32(shutterPositioningFieldMessageData.HighSpeedDurationClose);
            this.LowerSpeed = decimal.ToInt32(shutterPositioningFieldMessageData.LowerSpeed);
        }

        #endregion

        #region Properties

        public int HigherDistance { get; set; }

        public int HighSpeedDurationClose { get; set; }

        public int HighSpeedDurationOpen { get; set; }

        public int LowerDistance { get; set; }

        public int LowerSpeed { get; set; }

        public MovementType MovementType { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; }

        public int SpeedRate { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Direction:{this.ShutterMovementDirection.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
