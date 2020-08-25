using System;
using Ferretto.VW.CommonUtils;

namespace Ferretto.VW.Devices.BarcodeReader
{
    public interface IBarcodeReaderDriver
    {
        #region Events

        event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        void Connect(SerialPortOptions options);

        void Disconnect();
        void SimulateRead(string barcode);

        #endregion
    }
}
