namespace Ferretto.VW.Devices.BarcodeReader
{
    public class NewlandSerialPortOptions : SerialPortOptions
    {
        #region Constructors

        public NewlandSerialPortOptions()
        {
            this.BaudRate = FactorySettings.BaudRate;
            this.Parity = FactorySettings.Parity;
            this.StopBits = FactorySettings.StopBits;
        }

        #endregion

        #region Properties

        public DeviceModel DeviceModel { get; set; }

        #endregion
    }
}
