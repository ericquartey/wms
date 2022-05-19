using System;
using System.Collections.Generic;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.CanOpenClient;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportCan : ISocketTransportCan, IDisposable
    {
        #region Fields

        public const int UdpPollingInterval = 50;

        private const int idlePollingInterval = 1200;

        private const byte m_cPDO = 1;

        private readonly Timer implicitTimer;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private CanOpen client;

        private bool isDisposed;

        private ushort m_boardHandle;

        private int m_CANline;

        private Dictionary<int, byte[]> m_pdoInData;

        private Dictionary<int, byte[]> m_pdoOutData;

        private IEnumerable<int> nodeList;

        private DateTime receivedImplicitTime;

        #endregion

        #region Constructors

        public SocketTransportCan(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
            this.implicitTimer = new Timer(this.ImplicitTimer, null, -1, -1);
        }

        #endregion

        #region Events

        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<ImplicitReceivedEventArgs> ImplicitReceivedChanged;

        #endregion

        #region Properties

        public bool IsConnected { get; set; }

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort, IEnumerable<int> nodeList = null)
        {
            this.client = new CanOpen();
            this.client.SelectDevice();
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

        public bool ImplicitMessageWrite(byte[] data, int node)
        {
            var write = false;
            if (this.m_pdoOutData != null)
            {
                this.m_pdoOutData[node] = data;
                write = true;
            }
            return write;
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public bool SDOMessage(byte nodeId, ushort index, byte subindex, bool isWriteMessage, byte[] data, out byte[] receive, out int length)
        {
            receive = null;
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

        private void EmcyCallback(ushort boardhdl, byte que_num, byte canline)
        {
            throw new NotImplementedException();
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer?.Change(-1, -1);
            foreach (var nodeId in this.nodeList)
            {
                Int16 result;

                if (this.m_pdoOutData.ContainsKey(nodeId)
                    && this.m_pdoOutData[nodeId] != null
                    && this.m_pdoOutData[nodeId].Length > 0)
                {
                    //result = CANopenMasterAPI6.COP_WritePDO(this.m_boardHandle             //  handle of CAN board
                    //                                        , (byte)nodeId                  //  number of the node
                    //                                        , m_cPDO                    //  number of the pdo
                    //                                        , this.m_pdoOutData[nodeId]);           //  data to transmit
                }
            }
            this.implicitTimer?.Change(UdpPollingInterval, UdpPollingInterval);
        }

        private void PDOCallback(ushort boardhdl, byte que_num, byte canline)
        {
            throw new NotImplementedException();
        }

        private void StatusCallback(ushort boardhdl, byte que_num, byte canline)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
