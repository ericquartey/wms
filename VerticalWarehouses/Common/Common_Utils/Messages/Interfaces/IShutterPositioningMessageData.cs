using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IShutterPositioningMessageData : IMessageData
    {
        #region Properties

        int BayNumber { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterPosition ShutterPosition { get; }

        ShutterType ShutterType { get; }

        decimal SpeedRate { get; }

        #endregion
    }
}
