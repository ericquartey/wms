using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositioningFieldMessageData : IShutterPositioningFieldMessageData
    {
        #region Constructors

        public ShutterPositioningFieldMessageData(
            ShutterPosition shutterPosition,
            ShutterMovementDirection shutterMovementDirection,
            ShutterType shutterType,
            decimal speedRate,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
        }

        public ShutterPositioningFieldMessageData(IShutterPositioningMessageData messageData)
        {
            if (messageData == null)
            {
                throw new System.ArgumentNullException(nameof(messageData));
            }

            this.ShutterPosition = messageData.ShutterPosition;
            this.ShutterMovementDirection = messageData.ShutterMovementDirection;
            this.ShutterType = messageData.ShutterType;
            this.SpeedRate = messageData.SpeedRate;
            this.Verbosity = messageData.Verbosity;
        }

        #endregion

        #region Properties

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterPosition ShutterPosition { get; }

        public ShutterType ShutterType { get; }

        public decimal SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Position:{this.ShutterPosition.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
