using System.IO.Ports;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public class ConfigurationOptions : IBarcodeConfigurationOptions
    {
        #region Properties

        public int BaudRate { get; set; } = FactorySettings.BaudRate;

        public Parity Parity { get; set; } = FactorySettings.Parity;

        public string PortName { get; set; }

        public StopBits StopBits { get; set; } = FactorySettings.StopBits;

        #endregion
    }
}
