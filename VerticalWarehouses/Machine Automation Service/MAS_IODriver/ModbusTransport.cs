using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Ferretto.VW.MAS_IODriver.Interface;
using Modbus.Device;

namespace Ferretto.VW.MAS_IODriver
{
    public class ModbusTransport : IModbusTransport
    {
        #region Fields

        private IPAddress hostAddress;

        private TcpClient ioClient;

        private ModbusIpMaster ioMaster;

        private int port;

        #endregion

        #region Properties

        public bool IsConnected => this.ioClient?.Connected ?? false;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress hostAddress, int port)
        {
            this.hostAddress = hostAddress;
            this.port = port;
        }

        public bool Connect()
        {
            this.ioClient = new TcpClient(this.hostAddress.ToString(), this.port);
            this.ioMaster = ModbusIpMaster.CreateIp(this.ioClient);

            return this.ioClient.Connected;
        }

        public void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool[]> ReadAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync(bool[] outputs)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
