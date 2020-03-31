using System;
using System.Threading;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class MockReader : IBarcodeReader, IDisposable
    {
        #region Fields

        private bool isDisposed;

        private Timer timer;

        #endregion

        #region Events

        public event EventHandler<ActionEventArgs> BarcodeReceived;

        #endregion

        #region Methods

        public void Connect(IBarcodeConfigurationOptions options)
        {
            this.Disconnect();
            this.timer = new Timer(this.OnTimerTick, null, 10000, 10000);
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
            this.BarcodeReceived?.Invoke(this, new ActionEventArgs("L001F"));
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
