namespace Ferretto.VW.Devices.TokenReader
{
    public class TokenReaderSerialPortOptions : SerialPortOptions
    {
        #region Constructors

        public TokenReaderSerialPortOptions()
        {
            this.BaudRate = 9600;
            this.Parity = System.IO.Ports.Parity.Even;
            this.StopBits = System.IO.Ports.StopBits.One;
        }

        #endregion
    }
}
