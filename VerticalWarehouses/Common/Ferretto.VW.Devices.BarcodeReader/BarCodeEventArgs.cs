using System;

namespace Ferretto.VW.Devices.BarcodeReader
{
    public class BarcodeEventArgs
    {
        #region Constructors

        public BarcodeEventArgs(string barcode)
        {
            if (barcode is null)
            {
                throw new ArgumentNullException(nameof(barcode));
            }

            this.Barcode = barcode;
        }

        #endregion

        #region Properties

        public string Barcode { get; }

        #endregion
    }
}
