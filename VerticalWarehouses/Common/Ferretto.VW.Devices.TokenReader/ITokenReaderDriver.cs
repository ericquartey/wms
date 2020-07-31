namespace Ferretto.VW.Devices.TokenReader
{
    public interface ITokenReaderDriver
    {
        #region Events

        event System.EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion

        #region Methods

        void Connect(SerialPortOptions options);

        void Disconnect();

        #endregion
    }
}
