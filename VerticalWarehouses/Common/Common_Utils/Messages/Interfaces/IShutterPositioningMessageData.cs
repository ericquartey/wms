using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IShutterPositioningMessageData : IMessageData
    {
        #region Properties
        
        ShutterPosition ShutterPosition { get; }

        ShutterMovementDirection ShutterMovementDirection { get; }

        ShutterType ShutterType { get; }

        int BayNumber { get; }

        decimal SpeedRate { get; }
      
        #endregion
    }
}
