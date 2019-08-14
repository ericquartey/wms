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
        }

        #endregion

        #region Properties

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterPosition ShutterPosition { get; }

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
