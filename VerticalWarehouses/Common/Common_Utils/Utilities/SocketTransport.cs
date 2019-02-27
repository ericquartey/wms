using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Interfaces;

namespace Ferretto.VW.Common_Utils.Utilities
{
    public class SocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly byte[] receiveBuffer = new byte[1024];

        private bool disposed;

        private IPAddress hostAddress;

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

        public bool IsConnected => this.transportClient?.Connected ?? false && this.transportStream != null;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress hostAddress, int sendPort)
        {
            this.hostAddress = hostAddress;
            this.sendPort = sendPort;
        }

        /// <inheritdoc />
        public async Task<bool> ConnectAsync()
        {
            if (this.hostAddress == null)
                throw new ArgumentNullException(nameof(this.hostAddress),
                    $"{nameof(this.hostAddress)} can't be null");

            if (this.hostAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("Host Address is not a valid IPV4 address",
                    nameof(this.hostAddress));

            if (this.sendPort == 0)
                throw new ArgumentNullException(nameof(this.sendPort), $"{nameof(this.sendPort)} can't be zero");

            if (this.sendPort < 1024 || this.sendPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and 65535");

            if (this.transportClient != null || this.transportStream != null)
                throw new SocketTransportException("Socket Transport is already open",
                    SocketTransportExceptionCode.SocketOpen);

            try
            {
                this.transportClient = new TcpClient();
            }
            catch (Exception ex)
            {
                throw new SocketTransportException("Failed to create Transport Socket client",
                    SocketTransportExceptionCode.TcpClientCreationFailed, ex);
            }

            try
            {
                await this.transportClient.ConnectAsync(this.hostAddress, this.sendPort);
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new SocketTransportException("Failed to connect to Host Hardware",
                    SocketTransportExceptionCode.TcpInverterConnectionFailed, ex);
            }

            try
            {
                this.transportStream = this.transportClient.GetStream();
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new SocketTransportException("Failed to retrieve socket communication stream",
                    SocketTransportExceptionCode.GetNetworkStreamFailed, ex);
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
                throw new SocketTransportException("Transport Stream is null",
                    SocketTransportExceptionCode.UninitializedNetworkStream);

            if (!this.transportStream.CanRead)
                throw new SocketTransportException("Transport Stream not configured for reading data",
                    SocketTransportExceptionCode.MisconfiguredNetworkStream);

            try
            {
                await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new SocketTransportException("Error reading data from Transport Stream",
                    SocketTransportExceptionCode.NetworkStreamReadFailure, ex);
            }

            return this.receiveBuffer;
        }

        /// <inheritdoc />
        public async Task WriteAsync(byte[] hostMessage, CancellationToken stoppingToken)
        {
            if (this.transportStream == null)
                throw new SocketTransportException("Transport Stream is null",
                    SocketTransportExceptionCode.UninitializedNetworkStream);

            if (!this.transportStream.CanWrite)
                throw new SocketTransportException("Transport Stream not configured for sending data",
                    SocketTransportExceptionCode.MisconfiguredNetworkStream);

            try
            {
                await this.transportStream.WriteAsync(hostMessage, 0, hostMessage.Length, stoppingToken);
            }
            catch (Exception ex)
            {
                throw new SocketTransportException("Error writing data to Transport Stream",
                    SocketTransportExceptionCode.NetworkStreamWriteFailure, ex);
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
