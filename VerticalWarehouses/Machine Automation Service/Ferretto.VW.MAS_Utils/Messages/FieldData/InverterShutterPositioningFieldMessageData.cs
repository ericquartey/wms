using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterShutterPositioningFieldMessageData : IInverterShutterPositioningFieldMessageData
    {
        #region Constructors

        public InverterShutterPositioningFieldMessageData(IShutterPositioningFieldMessageData shutterPositioningFieldMessageData, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPositioningFieldMessageData.ShutterPosition;
            this.ShutterType = shutterPositioningFieldMessageData.ShutterType;
            this.ShutterMovementDirection = shutterPositioningFieldMessageData.ShutterMovementDirection;
            this.SpeedRate = decimal.ToInt32(shutterPositioningFieldMessageData.SpeedRate);
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public ShutterType ShutterType { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public int SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
