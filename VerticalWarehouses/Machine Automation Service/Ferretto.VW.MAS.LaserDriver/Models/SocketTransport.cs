using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal class SocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly int readTimeoutMilliseconds;   // -1 is no timeout

        private readonly byte[] receiveBuffer = new byte[1024];

        private IPAddress ioAddress;

        private bool isDisposed;

        private int sendPort;

        private TcpClient transportClient;

        private NetworkStream transportStream;

        #endregion

        #region Constructors

        public SocketTransport(int readTimeoutMilliseconds)
        {
            this.readTimeoutMilliseconds = readTimeoutMilliseconds;
        }

        #endregion

        #region Properties

        public bool IsConnected => this.transportClient?.Connected ?? false;

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task ConnectAsync(IPAddress hostAddress, int sendPort)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (hostAddress is null)
            {
                throw new ArgumentNullException(nameof(hostAddress));
            }

            if (hostAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("IODevice Address is not a valid IPV4 address", nameof(hostAddress));
            }

            if (sendPort < 1024 || sendPort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and {IPEndPoint.MaxPort}");
            }

            this.ioAddress = hostAddress;
            this.sendPort = sendPort;

            this.transportClient?.Dispose();
            this.transportStream?.Dispose();
            this.transportClient = null;
            this.transportStream = null;

            try
            {
                this.transportClient = new TcpClient();
            }
            catch (Exception ex)
            {
                throw new SocketTransportException(
                    "Failed to create Transport Socket client",
                    SocketTransportExceptionCode.TcpClientCreationFailed,
                    ex);
            }

            try
            {
                await this.transportClient.ConnectAsync(this.ioAddress, this.sendPort);
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new SocketTransportException(
                    "Failed to connect to IO Hardware",
                    SocketTransportExceptionCode.TcpInverterConnectionFailed,
                    ex);
            }

            try
            {
                this.transportStream = this.transportClient.GetStream();
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new SocketTransportException(
                    "Failed to retrieve socket communication stream",
                    SocketTransportExceptionCode.GetNetworkStreamFailed,
                    ex);
            }
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (!this.transportClient?.Connected ?? false)
            {
                return;
            }

            this.transportStream?.Close();
            this.transportClient?.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (this.transportStream == null)
            {
                throw new SocketTransportException(
                    "Transport Stream is null",
                    SocketTransportExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanRead)
            {
                throw new SocketTransportException(
                    "Transport Stream not configured for reading data",
                    SocketTransportExceptionCode.MisconfiguredNetworkStream);
            }

            byte[] receivedData;
            try
            {
                if (this.transportClient.Client.Poll(this.readTimeoutMilliseconds * 1000, SelectMode.SelectRead))
                {
                    var readBytes = await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);
                    if (readBytes > 0)
                    {
                        receivedData = new byte[readBytes];

                        Array.Copy(this.receiveBuffer, receivedData, readBytes);
                    }
                    else
                    {
                        this.Disconnect();
                        throw new SocketTransportException("Error reading data from Transport Stream");
                    }
                }
                else
                {
                    this.Disconnect();
                    throw new SocketTransportException("Timeout reading data from Transport Stream");
                }
            }
            catch (IoDriverException ex)
            {
                this.Disconnect();
                throw new SocketTransportException(ex.Message);
            }
            catch (IOException ex)
            {
                this.Disconnect();
                throw new SocketTransportException(ex.Message);
            }
            catch (Exception ex)
            {
                this.Disconnect();
                throw new SocketTransportException(
                    "Error reading data from Transport Stream",
                    SocketTransportExceptionCode.NetworkStreamReadFailure,
                    ex);
            }

            return receivedData;
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] message, CancellationToken stoppingToken)
        {
            return await this.WriteAsync(message, 0, stoppingToken);
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] message, int delay, CancellationToken stoppingToken)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (this.transportStream is null)
            {
                throw new SocketTransportException(
                    "Transport Stream is null",
                    SocketTransportExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanWrite)
            {
                throw new SocketTransportException(
                    "Transport Stream not configured for sending data",
                    SocketTransportExceptionCode.MisconfiguredNetworkStream);
            }

            if (!this.IsConnected)
            {
                throw new IoDriverException("Error writing data to Transport Stream", IoDriverExceptionCode.NetworkStreamWriteFailure);
            }

            try
            {
                if (delay > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                await this.transportStream.WriteAsync(message, 0, message.Length, stoppingToken);
                return message.Length;
            }
            catch (Exception ex)
            {
                this.Disconnect();
                throw new SocketTransportException("Error writing data to Transport Stream", SocketTransportExceptionCode.NetworkStreamWriteFailure, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
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

                this.isDisposed = true;
            }
        }

        #endregion
    }
}
