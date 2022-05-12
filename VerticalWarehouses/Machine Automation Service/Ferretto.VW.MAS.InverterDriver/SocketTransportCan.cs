using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;
using NLog;
using IXXAT;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportCan : ISocketTransportCan, IDisposable
    {
        #region Fields

        public const int UdpPollingInterval = 50;

        private const int idlePollingInterval = 1200;

        private const int udpPort = 0x8AE;

        private readonly Timer implicitTimer;

        private readonly IPAddress localAddress;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private bool isDisposed;

        private ushort m_boardHandle;

        private int m_CANline;

        #endregion

        #region Constructors

        public SocketTransportCan(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
            this.localAddress = IPAddress.Parse(configuration.GetValue("Vertimag:LocalAddress", "192.168.0.10"));
            this.implicitTimer = new Timer(this.ImplicitTimer, null, -1, -1);
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        public bool IsConnected => false;

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            var boardID = CANopenMasterAPI6.COP_1stBOARD;
            var boardType = CANopenMasterAPI6.COP_DEFAULTBOARD;
            var result = CANopenMasterAPI6.COP_InitBoard(out this.m_boardHandle, ref boardType, ref boardID, this.m_CANline);
            if (CANopenMasterAPI6.COP_k_OK != result)
            {
                throw new ApplicationException($"CANopenMasterAPI6: Error init DLL");
            }
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive, out int length)
        {
            throw new NotImplementedException();
        }

        public bool ImplicitMessageWrite(byte[] data)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
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
                this.implicitTimer?.Dispose();
            }
        }

        // this always arrives with a 50ms interval - even if ImplicitTimer is slower
        private void ImplicitMessageReceived(EnIPAttribut sender)
        {
            throw new NotImplementedException();
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer?.Change(-1, -1);
            throw new NotImplementedException();
        }

        #endregion
    }
}
