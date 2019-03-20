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
            this.Dispose(false);
        }

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

        /// <inheritdoc />
        public bool Connect()
        {
            try
            {
                this.ioClient = new TcpClient(this.hostAddress.ToString(), this.port);
            }
            catch (Exception ex)
            {
                throw new IoDriverException("Invalid hostAddress and port: remote endpoint not connected.", IoDriverExceptionCode.IoClientCreationFailed, ex);
            }

            try
            {
                this.ioMaster = ModbusIpMaster.CreateIp(this.ioClient);
            }
            catch (Exception ex)
            {
                throw new IoDriverException("Invalid IpMaster: remote endpoint not connected.", IoDriverExceptionCode.GetIpMasterFailed, ex);
            }

            return this.ioClient.Connected;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            this.ioClient?.Close();
            this.ioClient?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<bool[]> ReadAsync()
        {
            if (this.ioClient.Connected)
            {
                return await this.ioMaster.ReadInputsAsync(InputsAddress, InputsNomber);
            }

            throw new IoDriverException("Invalid Read request: remote endpoint not connected.", IoDriverExceptionCode.CreationFailure);
        }

        /// <inheritdoc />
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
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.ioClient.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
