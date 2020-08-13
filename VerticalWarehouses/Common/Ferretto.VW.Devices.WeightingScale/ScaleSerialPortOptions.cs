namespace Ferretto.VW.Devices.WeightingScale
{
    public class ScaleSerialPortOptions : SerialPortOptions
    {
        #region Constructors

        public ScaleSerialPortOptions()
        {
            this.BaudRate = 19200;
            this.Parity = System.IO.Ports.Parity.None;
            this.StopBits = System.IO.Ports.StopBits.One;
            this.ReadTimeout = System.TimeSpan.FromSeconds(1.2);
        }

        #endregion
    }
}
