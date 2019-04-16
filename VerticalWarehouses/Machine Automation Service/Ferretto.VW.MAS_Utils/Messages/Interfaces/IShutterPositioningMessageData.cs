namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IShutterPositioningMessageData : IMessageData
    {
        #region Properties

        int BayNumber { get; }

        int ShutterPositionMovement { get; }

        int ShutterType { get; }

        #endregion
    }
}
