using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly Stopwatch readStopwatch;

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private readonly byte[] receiveBuffer = new byte[1024];

        private readonly Stopwatch roundTripStopwatch;

        private IPAddress inverterAddress;

        private bool isDisposed;

        private int sendPort;

        private TcpClient transportClient;

        private NetworkStream transportStream;

        #endregion

        #region Constructors

        public SocketTransport(
            IConfiguration configuration
            )
        {
            this.readStopwatch = new Stopwatch();

            this.roundTripStopwatch = new Stopwatch();

            this.ReadWaitTimeData = new InverterDiagnosticsData();

            this.WriteRoundtripTimeData = new InverterDiagnosticsData();

            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:InverterDriver:ReadTimeoutMilliseconds", -1);
        }

        #endregion

        #region Properties

        public bool IsConnected => this.transportClient?.Connected ?? false;

        public InverterDiagnosticsData ReadWaitTimeData { get; }

        public InverterDiagnosticsData WriteRoundtripTimeData { get; }

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
            if (this.inverterAddress is null)
            {
                throw new ArgumentNullException(
                    nameof(this.inverterAddress),
                    $"{nameof(this.inverterAddress)} can't be null");
            }

            if (this.inverterAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "Inverter Address is not a valid IPV4 address",
                    nameof(this.inverterAddress));
            }

            if (this.sendPort == 0)
            {
                throw new ArgumentNullException(
                    nameof(this.sendPort),
                    $"{nameof(this.sendPort)} can't be zero");
            }

            if (this.sendPort < 1024 || this.sendPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and 65535");
            }

            if (this.transportClient != null || this.transportStream != null)
            {
                this.transportClient?.Dispose();
                this.transportStream?.Dispose();
                this.transportClient = null;
                this.transportStream = null;
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
            if (!this.transportClient?.Connected ?? false)
            {
                return;
            }

            this.transportStream?.Close();
            this.transportClient?.Close();

            //this.Dispose(true);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.transportStream is null)
            {
                throw new InverterDriverException(
                    "Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanRead)
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

            byte[] receivedData = null;
            try
            {
                this.readStopwatch.Reset();
                this.readStopwatch.Start();

                if (this.transportClient.Client?.Poll(this.readTimeoutMilliseconds * 1000, SelectMode.SelectRead) ?? false)
                {
                    var readBytes = await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);

                    this.readStopwatch.Stop();
                    this.roundTripStopwatch.Stop();
                    this.ReadWaitTimeData.AddValue(this.readStopwatch.ElapsedTicks);
                    this.WriteRoundtripTimeData.AddValue(this.roundTripStopwatch.ElapsedTicks);

                    if (readBytes > 0)
                    {
                        receivedData = new byte[readBytes];

                        Array.Copy(this.receiveBuffer, receivedData, readBytes);
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
            return await this.WriteAsync(inverterMessage, 0, stoppingToken);
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
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
                    this.roundTripStopwatch.Reset();
                    this.roundTripStopwatch.Start();
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

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

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

        #endregion
    }
}
