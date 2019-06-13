namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IShutterControlMessageData : IMessageData
    {
        #region Properties

        int BayNumber { get; set; }

        int CurrentShutterPosition { get; set; }

        int Delay { get; set; }

        int ExecutedCycles { get; set; }

        int NumberCycles { get; set; }

        int SpeedRate { get; set; }

        #endregion
    }
}
