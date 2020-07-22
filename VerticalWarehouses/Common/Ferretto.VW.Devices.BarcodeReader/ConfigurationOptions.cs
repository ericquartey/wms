using System.IO.Ports;

namespace Ferretto.VW.Devices.BarcodeReader
{
    public class ConfigurationOptions
    {
        #region Properties

        public int BaudRate { get; set; } = FactorySettings.BaudRate;

        public DeviceModel DeviceModel { get; set; }

        public Parity Parity { get; set; } = FactorySettings.Parity;

        public string PortName { get; set; }

        public StopBits StopBits { get; set; } = FactorySettings.StopBits;

        #endregion
    }
}
