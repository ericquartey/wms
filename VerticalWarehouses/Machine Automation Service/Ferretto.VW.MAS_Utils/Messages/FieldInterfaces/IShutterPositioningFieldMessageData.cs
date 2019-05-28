using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        ShutterPosition ShutterPosition { get; }

        ShutterType ShutterType { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        byte SystemIndex { get; set; }

        decimal SpeedRate { get; set; }

        #endregion
    }
}
