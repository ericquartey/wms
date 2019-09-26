using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IShutterPositioningMessageData : IMessageData
    {
        #region Properties

        int Delay { get; set; }

        int ExecutedCycles { get; set; }

        decimal HigherDistance { get; }

        decimal HighSpeedPercent { get; set; }

        decimal LowerDistance { get; }

        decimal LowerSpeed { get; }

        MovementMode MovementMode { get; set; }

        MovementType MovementType { get; }

        int RequestedCycles { get; set; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        decimal SpeedRate { get; }

        #endregion
    }
}
