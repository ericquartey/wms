using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.MAS_IODriver.Interface;
using Modbus.Device;

namespace Ferretto.VW.MAS_IODriver
{
    public class ModbusTransport : IModbusTransport
    {
        #region Fields

        private const ushort InputsAddress = 0;

        private const ushort InputsNomber = 8;

        private const ushort OutputAddress = 3;

        private bool disposed = false;

        private IPAddress hostAddress;

        private TcpClient ioClient;

        private ModbusIpMaster ioMaster;

        private int port;

        #endregion

        #region Destructors

        ~ModbusTransport()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsConnected => this.ioClient?.Connected ?? false;

        #endregion

        #region Methods

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
            this.ioClient?.Close();
            this.ioClient?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<bool[]> ReadAsync()
        {
            if (this.ioClient.Connected)
            {
                return await this.ioMaster.ReadInputsAsync(InputsAddress, InputsNomber);
            }

            throw new IoDriverException("Invalid Read request: remote endpoint not connected.", IoDriverExceptionCode.CreationFailure);
        }

        public async Task WriteAsync(bool[] outputs)
        {
            if (this.ioClient.Connected)
            {
                await this.ioMaster.WriteMultipleCoilsAsync(OutputAddress, outputs);
            }
            else
            {
                throw new IoDriverException("Invalid Write request: remote endpoint not connected.", IoDriverExceptionCode.CreationFailure);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                this.ioClient.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
