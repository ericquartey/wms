using System;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class BarcodeReaderService : IDisposable
    {
        #region Fields

        private bool isDisposed;

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.StopAsync();
                this.wmsStatusTimer.Dispose();
            }

            this.isDisposed = true;
        }

        #endregion
    }
}
