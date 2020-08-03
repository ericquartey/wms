using System.IO.Ports;

namespace Ferretto.VW.Devices
{
    public class SerialPortOptions
    {
        #region Properties

        public int BaudRate { get; set; }

        public Parity Parity { get; set; }

        public string PortName { get; set; }

        public System.TimeSpan? ReadTimeout { get; set; }

        public StopBits StopBits { get; set; }

        #endregion
    }
}
