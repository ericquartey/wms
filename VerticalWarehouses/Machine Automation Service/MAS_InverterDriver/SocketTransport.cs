using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.InverterDriver
{
    public class SocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly byte[] receiveBuffer = new byte[1024];

        private bool disposed;

        private IPAddress inverterAddress;

        private int sendPort;

        private TcpClient transportClient;

        private NetworkStream transportStream;

        #endregion

        #region Destructors

        ~SocketTransport()
        {
            this.Dispose(true);
        }

        #endregion

        #region Properties

        public bool IsConnected => this.transportClient?.Connected ?? false;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.inverterAddress = inverterAddress;
            this.sendPort = sendPort;
        }

        /// <inheritdoc />
        public async Task<bool> ConnectAsync()
        {
            if (this.inverterAddress == null)
                throw new ArgumentNullException(nameof(this.inverterAddress),
                    $"{nameof(this.inverterAddress)} can't be null");

            if (this.inverterAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("Inverter Address is not a valid IPV4 address",
                    nameof(this.inverterAddress));

            if (this.sendPort == 0)
                throw new ArgumentNullException(nameof(this.sendPort), $"{nameof(this.sendPort)} can't be zero");

            if (this.sendPort < 1024 || this.sendPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and 65535");

            if (this.transportClient != null || this.transportStream != null)
                throw new InverterDriverException("Socket Transport is already open",
                    InverterDriverExceptionCode.SocketOpen);

            try
            {
                this.transportClient = new TcpClient();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException("Failed to create Transport Socket client",
                    InverterDriverExceptionCode.TcpClientCreationFailed, ex);
            }

            try
            {
                await this.transportClient.ConnectAsync(this.inverterAddress, this.sendPort);
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new InverterDriverException("Failed to connect to Inverter Hardware",
                    InverterDriverExceptionCode.TcpInverterConnectionFailed, ex);
            }

            try
            {
                this.transportStream = this.transportClient.GetStream();
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new InverterDriverException("Failed to retrieve socket communication stream",
                    InverterDriverExceptionCode.GetNetworkStreamFailed, ex);
            }

            return this.transportClient?.Connected ?? false;
        }

        /// <inheritdoc />
        public void Disconnect(int delay)
        {
            if (!this.transportClient?.Connected ?? false) return;

            this.transportStream?.Close(delay);
            this.transportClient?.Close();

            this.Dispose(true);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.transportStream == null)
                throw new InverterDriverException("Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);

            if (!this.transportStream.CanRead)
                throw new InverterDriverException("Transport Stream not configured for reading data",
                    InverterDriverExceptionCode.MisconfiguredNetworkStream);

            try
            {
                await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException("Error reading data from Transport Stream",
                    InverterDriverExceptionCode.NetworkStreamReadFailure, ex);
            }

            return this.receiveBuffer;
        }

        /// <inheritdoc />
        public async Task WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            if (this.transportStream == null)
                throw new InverterDriverException("Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);

            if (!this.transportStream.CanWrite)
                throw new InverterDriverException("Transport Stream not configured for sending data",
                    InverterDriverExceptionCode.MisconfiguredNetworkStream);

            try
            {
                await this.transportStream.WriteAsync(inverterMessage, 0, inverterMessage.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException("Error writing data to Transport Stream",
                    InverterDriverExceptionCode.NetworkStreamWriteFailure, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.transportStream?.Close();
                    this.transportStream?.Dispose();
                    this.transportStream = null;

                    this.transportClient?.Close();
                    this.transportClient?.Dispose();
                    this.transportClient = null;
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
