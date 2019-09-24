using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        decimal HigherDistance { get; }

        decimal HighSpeedPercent { get; set; }

        decimal LowerDistance { get; }

        decimal LowerSpeed { get; }

        MovementType MovementType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        decimal SpeedRate { get; set; }

        #endregion
    }
}
