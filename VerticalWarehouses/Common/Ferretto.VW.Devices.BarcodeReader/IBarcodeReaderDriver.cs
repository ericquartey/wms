using System;

namespace Ferretto.VW.Devices.BarcodeReader
{
    public interface IBarcodeReaderDriver
    {
        #region Events

        event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        void Connect(IBarcodeConfigurationOptions options);

        void Disconnect();

        #endregion
    }
}
