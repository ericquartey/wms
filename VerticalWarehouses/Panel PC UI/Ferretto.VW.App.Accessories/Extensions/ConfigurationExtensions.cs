using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO.Ports;
using System.Linq;

namespace Ferretto.VW.App.Accessories
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string BaudRateKey = "Devices:BarcodeReader:BaudRate";

        private const string DefaultBaudRate = "115200";

        #endregion

        #region Methods

        public static int? GetBarcodeReaderBaudRate(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var baudRateString = appSettings.Get(BaudRateKey);

            if (int.TryParse(baudRateString, out var baudRate))
            {
                return baudRate;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
