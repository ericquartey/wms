using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.EnIPStack;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private readonly byte[] receiveBuffer = new byte[1024];

        private IPAddress inverterAddress;

        private bool isDisposed;

        private int sendPort;

        private TcpClient transportClient;

        private NetworkStream transportStream;

        #endregion

        #region Constructors

        public SocketTransport(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        public bool IsConnected => this.transportClient?.Connected ?? false;

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.inverterAddress = inverterAddress;
            this.sendPort = sendPort;
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            Trace.Assert(this.inverterAddress != null, $"{nameof(this.inverterAddress)} can't be null");

            Trace.Assert(this.inverterAddress.AddressFamily == AddressFamily.InterNetwork, "Inverter Address is not a valid IPV4 address");

            Trace.Assert(this.sendPort != 0, $"{nameof(this.sendPort)} can't be zero");

            Trace.Assert(this.sendPort >= 1024 && this.sendPort <= IPEndPoint.MaxPort, $"{nameof(this.sendPort)} value must be between 1024 and {IPEndPoint.MaxPort}");

            if (this.transportClient != null || this.transportStream != null)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
            }

            try
            {
                this.transportClient = new TcpClient();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException(
                    "Failed to create Transport Socket client",
                    InverterDriverExceptionCode.TcpClientCreationFailed,
                    ex);
            }

            try
            {
                await this.transportClient.ConnectAsync(this.inverterAddress, this.sendPort);
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new InverterDriverException(
                    "Failed to connect to Inverter Hardware",
                    InverterDriverExceptionCode.TcpInverterConnectionFailed,
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
                throw new InverterDriverException(
                    "Failed to retrieve socket communication stream",
                    InverterDriverExceptionCode.GetNetworkStreamFailed,
                    ex);
            }
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

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

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive)
        {
            throw new NotImplementedException();
        }

        public bool ImplicitMessageStart(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Array.Empty<byte>();
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

            var currentTransportStream = this.transportStream;
            if (currentTransportStream is null)
            {
                throw new InverterDriverException(
                    "Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!currentTransportStream.CanRead)
            {
                throw new InverterDriverException(
                    "Transport Stream not configured for reading data",
                    InverterDriverExceptionCode.MisconfiguredNetworkStream);
            }

            if (!this.IsConnected)
            {
                throw new InverterDriverException(
                    "Connection is not open.",
                    InverterDriverExceptionCode.NetworkStreamWriteFailure);
            }

            var currentReceiveBuffer = this.receiveBuffer;
            if (currentReceiveBuffer is null)
            {
                throw new InverterDriverException(
                    "Receive buffer is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            byte[] receivedData;
            try
            {
                if (this.transportClient.Client?.Poll(this.readTimeoutMilliseconds * 1000, SelectMode.SelectRead) ?? false)
                {
                    var readBytes = await currentTransportStream.ReadAsync(currentReceiveBuffer, 0, currentReceiveBuffer.Length, stoppingToken);

                    if (readBytes > 0)
                    {
                        receivedData = new byte[readBytes];

                        Array.Copy(currentReceiveBuffer, receivedData, readBytes);
                    }
                    else
                    {
                        this.Disconnect();
                        throw new InvalidOperationException("Error reading data from Transport Stream");
                    }
                }
                else
                {
                    this.Disconnect();
                    throw new InvalidOperationException("Timeout reading data from Transport Stream");
                }
            }
            catch (IOException ex)
            {
                this.Disconnect();
                throw new InvalidOperationException(ex.Message);
            }
            catch
            {
                this.Disconnect();
                throw;
            }

            return receivedData;
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

            return await this.WriteAsync(inverterMessage, 0, stoppingToken);
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

            if (inverterMessage is null)
            {
                throw new InverterDriverException(
                    "Inverter message is null",
                    InverterDriverExceptionCode.InverterPacketMalformed);
            }

            if (this.transportStream is null)
            {
                throw new InverterDriverException(
                    "Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanWrite)
            {
                throw new InverterDriverException(
                    "Transport Stream not configured for sending data",
                    InverterDriverExceptionCode.MisconfiguredNetworkStream);
            }

            if (!this.IsConnected)
            {
                throw new InverterDriverException(
                    "Connection is not open.",
                    InverterDriverExceptionCode.NetworkStreamWriteFailure);
            }

            try
            {
                if (delay > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                await this.transportStream.WriteAsync(inverterMessage, 0, inverterMessage.Length, stoppingToken);
                return inverterMessage.Length;
            }
            catch (Exception ex)
            {
                this.Disconnect();
                throw new InverterDriverException(
                    "Error writing data to Transport Stream",
                    InverterDriverExceptionCode.NetworkStreamWriteFailure,
                    ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (disposing)
            {
                this.transportStream?.Close();
                this.transportStream?.Dispose();
                this.transportStream = null;

                this.transportClient?.Close();
                this.transportClient?.Dispose();
                this.transportClient = null;
            }
        }

        #endregion
    }
}
