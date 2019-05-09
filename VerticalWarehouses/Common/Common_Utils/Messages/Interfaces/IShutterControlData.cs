namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IShutterControlMessageData : IMessageData
    {
        #region Properties

        int CurrentShutterPosition { get; set; }

        int Delay { get; set; }

        int NumberCycles { get; set; }

        #endregion
    }
}
