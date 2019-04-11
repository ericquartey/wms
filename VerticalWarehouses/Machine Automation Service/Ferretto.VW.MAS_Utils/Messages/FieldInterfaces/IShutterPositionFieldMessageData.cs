using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IShutterPositionFieldMessageData : IFieldMessageData
    {
        #region Properties

        ShutterPosition ShutterPosition { get; }

        #endregion
    }
}
