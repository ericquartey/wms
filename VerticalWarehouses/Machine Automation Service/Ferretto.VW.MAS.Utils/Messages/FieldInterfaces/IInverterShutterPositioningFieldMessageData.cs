using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        int HighSpeedDurationClose { get; set; }

        int HighSpeedDurationOpen { get; set; }

        int? HighSpeedHalfDurationClose { get; set; }

        int? HighSpeedHalfDurationOpen { get; set; }

        int LowerSpeed { get; set; }

        short MovementDuration { get; set; }

        MovementType MovementType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; set; }

        ShutterType ShutterType { get; }

        int SpeedRate { get; set; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
