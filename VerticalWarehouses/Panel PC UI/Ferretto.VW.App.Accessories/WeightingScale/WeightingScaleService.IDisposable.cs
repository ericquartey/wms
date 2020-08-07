using System;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class WeightingScaleService : IDisposable
    {
        #region Fields

        private bool isDisposed;

        #endregion

        #region Methods

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.StopAsync();

            this.weightPollTimer.Dispose();

            this.isDisposed = true;
        }

        #endregion
    }
}
