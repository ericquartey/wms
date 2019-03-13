using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        private readonly byte[] receiveBuffer = new byte[1024];

        private const ushort InputsAddress = 0;

        private const ushort InputsNomber = 8;

        private const ushort OutputAddress = 3;

        private bool disposed = false;

        private IPAddress hostAddress;

        private TcpClient ioClient;

        private ModbusIpMaster ioMaster;

        private NetworkStream transportStream;

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

            catch(Exception ex)
            {
                throw new IoDriverException("Invalid hostAddress and port: remote endpoint not connected.", IoDriverExceptionCode.IoClientCreationFailed);
            }

            try
            {
                this.ioMaster = ModbusIpMaster.CreateIp(this.ioClient);
            }

            catch (Exception ex)
            {
                throw new IoDriverException("Invalid IpMaster: remote endpoint not connected.", IoDriverExceptionCode.GetIpMasterFailed);
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<bool[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.ioClient.Connected)
            {
                /*return await*/ this.ioMaster.ReadInputsAsync(InputsAddress, InputsNomber);
            }

            else
            {
                throw new IoDriverException("Invalid Read request: remote endpoint not connected.", IoDriverExceptionCode.CreationFailure);
            }             

            try 
            {
                await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                

                throw new IoDriverException("Error reading data from Transport Stream", IoDriverExceptionCode.NetworkStreamReadFailure, ex);
            }

            return this.receiveBuffer;
        }

        /// <inheritdoc />
        public async Task WriteAsync(bool[] outputs, CancellationToken stoppingToken)
        {
            if (this.ioClient.Connected)
            {
                /*return await*/ this.ioMaster.WriteMultipleCoilsAsync(OutputAddress, outputs);
            }
            else
            {
                throw new IoDriverException("Invalid Write request: remote endpoint not connected.", IoDriverExceptionCode.CreationFailure);
            }

            try
            {
                await this.transportStream.WriteAsync(outputs, 0, outputs.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new IoDriverException("Error writing data to Transport Stream", IoDriverExceptionCode.NetworkStreamWriteFailure, ex);
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
