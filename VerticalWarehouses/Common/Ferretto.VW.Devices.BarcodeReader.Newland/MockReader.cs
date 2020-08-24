using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ferretto.VW.CommonUtils;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class MockReader : IBarcodeReaderDriver, IQueryableDevice, IDisposable
    {
        #region Fields

        private readonly IList<string> barcodes;

        private readonly int intervalMilliseconds;

        private int barcodeIndex;

        private bool isDisposed;

        private Timer timer;

        #endregion

        #region Constructors

        public MockReader(
            IList<string> barcodes,
            int intervalMilliseconds)
        {
            if (barcodes is null)
            {
                throw new ArgumentNullException(nameof(barcodes));
            }

            if (!barcodes.Any())
            {
                throw new ArgumentException(nameof(barcodes));
            }

            if (intervalMilliseconds <= 0)
            {
                throw new ArgumentException(nameof(intervalMilliseconds));
            }

            this.barcodes = barcodes;
            this.intervalMilliseconds = intervalMilliseconds;
        }

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Properties

        public DeviceInformation Information => new DeviceInformation
        {
            FirmwareVersion = "0.0.0.1-Mock",
            ModelNumber = "Newland 1553",
            ManufactureDate = DateTime.Now.Subtract(TimeSpan.FromDays(95)),
            SerialNumber = "1023841985"
        };

        #endregion

        #region Methods

        public void Connect(SerialPortOptions options)
        {
            this.Disconnect();
            this.barcodeIndex = 0;

            this.timer = new Timer(this.OnTimerTick, null, this.intervalMilliseconds, this.intervalMilliseconds);
        }

        public void Disconnect()
        {
            this.timer?.Dispose();
            this.timer = null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnTimerTick(object state)
        {
            var index = this.barcodeIndex;
            this.barcodeIndex = (this.barcodeIndex + 1) % this.barcodes.Count;

            this.BarcodeReceived?.Invoke(this, new ActionEventArgs(this.barcodes[index]));
        }

        public void SimulateRead(string barcode)
        {
            this.BarcodeReceived?.Invoke(this, new ActionEventArgs(barcode));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Disconnect();
                }

                this.isDisposed = true;
            }
        }

        #endregion
    }
}
