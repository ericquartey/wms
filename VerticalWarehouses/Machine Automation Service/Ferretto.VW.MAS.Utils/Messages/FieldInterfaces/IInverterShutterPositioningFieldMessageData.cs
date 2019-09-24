using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        int HigherDistance { get; set; }

        int HighSpeedPercent { get; set; }

        int LowerDistance { get; set; }

        int LowerSpeed { get; set; }

        MovementType MovementType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        int SpeedRate { get; set; }

        #endregion
    }
}
