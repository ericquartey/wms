using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        ShutterPosition ShutterPosition { get; }

        ShutterType ShutterType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        decimal SpeedRate { get; set; }

        #endregion
    }
}
