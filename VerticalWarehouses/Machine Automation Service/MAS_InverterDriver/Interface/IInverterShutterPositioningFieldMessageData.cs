using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        ShutterPosition ShutterPosition { get; }

        ShutterType ShutterType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        int SpeedRate { get; set; }

        #endregion
    }
}
