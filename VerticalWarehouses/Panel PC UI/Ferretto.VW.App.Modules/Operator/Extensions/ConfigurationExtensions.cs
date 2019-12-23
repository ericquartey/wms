using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO.Ports;
using System.Linq;

namespace Ferretto.VW.App.Modules.Operator
{
    public static class ConfigurationExtensions
    {
        #region Fields

        private const string BaudRateKey = "Devices:BarcodeReader:BaudRate";

        private const string DefaultBaudRate = "115200";

        private const string SerialPortNameKey = "Devices:BarcodeReader:SerialPortName";

        #endregion

        #region Methods

        public static int GetBarcodeReaderBaudRate(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var baudRateString = appSettings.Get(BaudRateKey) ?? DefaultBaudRate;

            if (int.TryParse(baudRateString, out var baudRate))
            {
                return baudRate;
            }
            else
            {
                throw new ConfigurationErrorsException();
            }
        }

        public static string GetBarcodeReaderSerialPortName(this NameValueCollection appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            var serialPortName = appSettings.Get(SerialPortNameKey) ?? SerialPort.GetPortNames().FirstOrDefault();

            return serialPortName;
        }

        #endregion
    }
}
