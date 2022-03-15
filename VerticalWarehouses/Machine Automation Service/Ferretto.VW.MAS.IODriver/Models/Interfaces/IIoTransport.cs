using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Exceptions;

namespace Ferretto.VW.MAS.IODriver
{
    internal interface IIoTransport : IDisposable
    {
        #region Properties

        /// <summary>
        ///     Returns Socket Transport connection status
        /// </summary>
        bool IsConnected { get; }

        int ReadTimeout { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the TCP client object and connects it to the remote IODevice host
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
        /// <exception cref="IoDriverException">Connection operation Failed. Inspect exception details for more details</exception>
        Task ConnectAsync(IPAddress ipAddress, int port);

        /// <summary>
        ///     Disconnects from the remote host and closes communication sockets
        /// </summary>
        void Disconnect();

        /// <summary>
        ///     Reads data from the remote host. Blocks the calling thread until new data is received from the host
        /// </summary>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <returns>A byte array containing the bytes read from the socket stream</returns>
        /// <exception cref="IoDriverException">Read operation Failed. Inspect exception details for more details</exception>
        /// <exception cref="ObjectDisposedException">Thrown when calling this method after the object has been disposed.</exception>
        ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken);

        /// <summary>
        ///     Sends data to the remote host asynchronously.
        /// </summary>
        /// <param name="dataMessage">Byte array containing the message to be sent to the IO device hardware</param>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <exception cref="IoDriverException">Write operation Failed. Inspect exception details for more details</exception>
        /// <exception cref="ObjectDisposedException">Thrown when calling this method after the object has been disposed.</exception>
        ValueTask<int> WriteAsync(byte[] dataMessage, CancellationToken stoppingToken);

        /// <summary>
        ///     Sends data to the remote host asynchronously.
        /// </summary>
        /// <param name="dataMessage">Byte array containing the message to be sent to the IO device hardware</param>
        /// <param name="delay">Time on waiting to send data (ms).</param>
        /// <param name="stoppingToken">Cancellation token used to cancel wait operations</param>
        /// <exception cref="IoDriverException">Write operation Failed. Inspect exception details for more details</exception>
        /// <exception cref="ObjectDisposedException">Thrown when calling this method after the object has been disposed.</exception>
        ValueTask<int> WriteAsync(byte[] dataMessage, int delay, CancellationToken stoppingToken);

        #endregion
    }
}
