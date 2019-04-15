namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IShutterControlData : IMessageData
    {
        #region Properties

        int Delay { get; set; }

        int NumberCycles { get; set; }

        #endregion
    }
}
