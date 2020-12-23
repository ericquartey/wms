using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IShutterPositioningMessageData : IMessageData
    {
        #region Properties

        bool BypassConditions { get; set; }

        int Delay { get; set; }

        double HighSpeedDurationClose { get; }

        double HighSpeedDurationOpen { get; }

        double? HighSpeedHalfDurationClose { get; }

        double? HighSpeedHalfDurationOpen { get; }

        double LowerSpeed { get; }

        MovementMode MovementMode { get; set; }

        MovementType MovementType { get; }

        int PerformedCycles { get; set; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        double SpeedRate { get; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
