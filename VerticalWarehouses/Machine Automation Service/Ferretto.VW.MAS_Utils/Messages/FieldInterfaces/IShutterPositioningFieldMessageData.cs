using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IShutterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        ShutterPosition ShutterPosition { get; }

        ShutterMovementDirection ShutterPositionMovement { get; }

        byte SystemIndex { get; set; }

        #endregion
    }
}
