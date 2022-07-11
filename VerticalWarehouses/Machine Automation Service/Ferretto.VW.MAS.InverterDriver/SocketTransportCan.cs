using System;
using System.Collections.Generic;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Microsoft.Extensions.Configuration;
using NLog;
using Ferretto.VW.MAS.CanOpenClient;

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportCan : ISocketTransportCan, IDisposable
    {
        #region Fields

        public const int UdpPollingInterval = 50;

        private const int idlePollingInterval = 1200;

        private const byte m_cPDO = 1;

        private readonly CanOpen client;

        private readonly Timer implicitTimer;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private bool isDisposed;

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
            this.client = new CanOpen();
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
            if (nodeList is null)
            {
                throw new ApplicationException($"nodeList is null");
            }
            this.nodeList = nodeList;
            try
            {
                this.client.SelectDevice(this.readTimeoutMilliseconds);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"CanOpen.SelectDevice error {ex.Message}");
            }
            var isOk = this.client.InitSocket(0, nodeList);
            if (!isOk)
            {
                this.client.FinalizeApp();
                throw new ApplicationException($"CanOpen.InitSocket error");
            }

            this.m_pdoOutData = new Dictionary<int, byte[]>();
            this.m_pdoInData = new Dictionary<int, byte[]>();

            foreach (var nodeId in nodeList)
            {
                this.m_pdoOutData.Add(nodeId, new byte[8]);
                this.m_pdoInData.Add(nodeId, new byte[8]);
            }
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            this.client.ImplicitMessageEvent += new ImplicitMessageEventHandler(this.ImplicitMessageReceived);
            this.IsConnected = true;
            this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.IsConnected, this.IsConnectedUdp));
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            try
            {
                this.client.SelectDevice(this.readTimeoutMilliseconds);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"CanOpen.SelectDevice error {ex.Message}");
            }
            var isOk = this.client.InitSocket(0, this.nodeList);
            if (!isOk)
            {
                this.client.FinalizeApp();
                throw new ApplicationException($"CanOpen.InitSocket error");
            }
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            this.IsConnected = true;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            this.client.Stop();
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
            if (this.m_pdoOutData != null
                && this.IsConnected)
            {
                this.m_pdoOutData[node] = data;
                if (!this.client.WritePDO((byte)node, this.m_pdoOutData[node]))
                {
                    throw new ApplicationException($"CanOpen.WritePDO error");
                }
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
            var isOk = false;
            ulong abortCode = 0;
            receive = null;
            length = 0;
            if (isWriteMessage)
            {
                isOk = this.client.WriteSDO(nodeId, index, subindex, data, (ushort)data.Length, out abortCode);
            }
            else
            {
                isOk = this.client.ReadSDO(nodeId, index, subindex, out receive, out length, out abortCode);
            }
            if (isOk)
            {
                this.logger.Trace("Node: " + nodeId
                                + $" object 0x{index:X04}/{subindex}"
                                + (isWriteMessage ? $" tx {BitConverter.ToString(data)} " : $"  rx {BitConverter.ToString(receive)}"));
            }
            else
            {
                length = (int)abortCode;
            }
            return isOk;
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

        private void ImplicitMessageReceived(CanOpen sender)
        {
            while (sender.GetPDO(out var node, out var data))
            {
                var args = new ImplicitReceivedEventArgs();
                args.receivedMessage = data;
                args.node = node;
                args.isOk = true;
                this.receivedImplicitTime = DateTime.UtcNow;
                this.ImplicitReceivedChanged?.Invoke(this, args);
            }
            while (sender.GetEmergency(out var node,
                out var error,
                out var register,
                out var manufacturerError))
            {
                var args = new ImplicitReceivedEventArgs();
                args.isEmergency = node > 0;
                args.node = node;
                if (args.isEmergency)
                {
                    args.emergencyError = error;
                    args.emergencyManufacturerError = manufacturerError;
                    args.emergencyRegister = register;
                    args.emergencyNode = node;
                }
                this.receivedImplicitTime = DateTime.UtcNow;
                this.ImplicitReceivedChanged?.Invoke(this, args);
            }
            while (sender.GetNMT(out var node,
                out var state,
                out var isSync))
            {
                var args = new ImplicitReceivedEventArgs();
                args.isNMT = node > 0;
                args.isSync = isSync;
                args.node = node;
                if (args.isNMT)
                {
                    args.nMTNode = node;
                    args.nMTState = state;
                }
                this.receivedImplicitTime = DateTime.UtcNow;
                this.ImplicitReceivedChanged?.Invoke(this, args);
            }
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer?.Change(-1, -1);
            foreach (var nodeId in this.nodeList)
            {
                if (this.m_pdoOutData.ContainsKey(nodeId)
                    && this.m_pdoOutData[nodeId] != null
                    && this.m_pdoOutData[nodeId].Length > 0
                    && this.IsConnected)
                {
                    if (!this.client.WritePDO((byte)nodeId, this.m_pdoOutData[nodeId]))
                    {
                        throw new ApplicationException($"CanOpen.WritePDO error");
                    }
                }
            }
            this.implicitTimer?.Change(UdpPollingInterval, UdpPollingInterval);
        }

        #endregion
    }
}
