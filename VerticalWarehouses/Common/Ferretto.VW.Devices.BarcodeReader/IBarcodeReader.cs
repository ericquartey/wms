using System;

namespace Ferretto.VW.Devices.BarcodeReader
{
    public interface IBarcodeReader
    {
        #region Events

        event EventHandler<BarcodeEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        void Connect(IBarcodeConfigurationOptions options);

        void Disconnect();

        #endregion
    }
}
