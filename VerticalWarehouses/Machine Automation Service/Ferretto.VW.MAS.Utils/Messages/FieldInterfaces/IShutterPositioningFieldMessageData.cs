using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        double HighSpeedDurationClose { get; }

        double HighSpeedDurationOpen { get; }

        double? HighSpeedHalfDurationClose { get; }

        double? HighSpeedHalfDurationOpen { get; }

        double LowerSpeed { get; }

        MovementType MovementType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        double SpeedRate { get; set; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
