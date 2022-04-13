using System;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.NordDriver
{
    public interface ISocketTransport
    {
        #region Events

        event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        /// <summary>
        ///     Returns Socket Transport connection status
        /// </summary>
        bool IsConnected { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Configures the SocketTransport to communicate with the remote inverter host
        /// </summary>
        /// <param name="inverterAddress">Address of the Inverter device</param>
        /// <param name="sendPort">TCP/IP Port for the Inverter device</param>
        void Configure(IPAddress inverterAddress, int sendPort, IPAddress localAddress);

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

        bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive);

        bool StartImplicitMessages();

        #endregion
    }
}
