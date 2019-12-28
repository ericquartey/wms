using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class MockReader : IBarcodeReader, IDisposable
    {
        private bool isDisposed;

        private Timer timer;

        public event EventHandler<BarcodeEventArgs> BarcodeReceived;

        public void Connect(IBarcodeConfigurationOptions options)
        {
            this.Disconnect();
            this.timer = new Timer(this.OnTimerTick, null, 10000, 10000);
        }

        public void OnTimerTick(object state)
        {
            this.BarcodeReceived?.Invoke(this, new BarcodeEventArgs("L123F"));
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
    }
}
