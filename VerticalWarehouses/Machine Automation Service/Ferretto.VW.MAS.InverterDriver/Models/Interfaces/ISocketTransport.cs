using System;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.InverterDriver.Interface
{
    public interface ISocketTransport
    {
        #region Events

        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        /// <summary>
        ///     Returns Socket Transport connection status
        /// </summary>
        bool IsConnected { get; }

        bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Configures the SocketTransport to communicate with the remote inverter host
        /// </summary>
        /// <param name="inverterAddress">Address of the Inverter device</param>
        /// <param name="sendPort">TCP/IP Port for the Inverter device</param>
        void Configure(IPAddress inverterAddress, int sendPort);

        /// <summary>
        ///     Creates the TCP client object and connects it to the remote inverter host
        /// </summary>
        /// <returns>true if the connection successfully completed, false otherwise</returns>
        /// <exception cref="ArgumentException">
        ///     IP Address specified during object configuration is not recognized as a valid IP V4
        ///     address
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Either inverterAddress or sendPort configuration values are null. Inspect
        ///     exception details for specific null parameter
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Port specified during object configuration is out of allowed range
        ///     (1024-65535)
        /// </exception>
        /// <exception cref="InverterDriverException">Connection operation Failed. Inspect exception details for more details</exception>
        Task ConnectAsync();

        /// <summary>
        ///     Disconnects from the remote host and closes communication sockets
        /// </summary>
        void Disconnect();

        bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive, out int length);

        bool ImplicitMessageStart(byte[] data);

        /// <summary>
        ///     Reads data from the remote host. Blocks the calling thread until new data is received from the host
        /// </summary>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <returns>A byte array containing the bytes read from the socket stream</returns>
        /// <exception cref="InverterDriverException">Read operation Failed. Inspect exception details for more details</exception>
        ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken);

        /// <summary>
        ///     Sends data to the remote host asynchronously.
        /// </summary>
        /// <param name="inverterMessage">Byte array containing the message to be sent to the Inverter hardware</param>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <exception cref="InverterDriverException">Write operation Failed. Inspect exception details for more details</exception>
        ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken);

        /// <summary>
        ///     Sends data to the remote host asynchronously.
        /// </summary>
        /// <param name="inverterMessage">Byte array containing the message to be sent to the Inverter hardware</param>
        /// <param name="delay">Time on waiting to send data (ms).</param>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <exception cref="InverterDriverException">Write operation Failed. Inspect exception details for more details</exception>
        ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken);

        #endregion
    }
}
