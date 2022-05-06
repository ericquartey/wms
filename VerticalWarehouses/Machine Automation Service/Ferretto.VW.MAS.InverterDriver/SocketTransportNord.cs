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

namespace Ferretto.VW.MAS.InverterDriver
{
    public class SocketTransportNord : ISocketTransport, IDisposable
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

        private EnIPClient client;

        private ForwardClose_Packet closePacket;

        private EnIPAttribut Input;

        private IPAddress inverterAddress;

        private bool isDisposed;

        private EnIPAttribut Output;

        private byte[] receiveBuffer = new byte[1024];

        private DateTimeOffset receivedImplicitTime;

        private EnIPRemoteDevice remoteDevice;

        // 0xAF12;
        private int sendPort;

        #endregion

        #region Constructors

        public SocketTransportNord(IConfiguration configuration)
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

        #region Enums

        private enum CipLogicalType : byte
        {
            ClassID = 0,

            InstanceID = 4,

            MemberID = 8,

            ConnectionPoint = 12,

            AttributeId = 16,

            Special = 20,

            ServiceID = 24,

            ExtendedLogical = 28
        }

        private enum CipSegmentTypes : byte
        {
            PortSegment = 0,

            LogicalSegment = 32,

            NetwortkSegment = 64,

            SymbolicSegment = 96,

            DataSegment = 128,

            DataTypeC62 = 160,

            DataTypeC61 = 192,

            Reserved = 224
        }

        private enum CipSize : byte
        {
            U8 = 0,

            U16 = 1,

            U32 = 2,

            Reserved = 3
        }

        #endregion

        #region Properties

        public bool IsConnected => this.remoteDevice?.IsConnected() ?? false;

        public bool IsConnectedUdp { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
            this.IsConnectedUdp = false;
            this.inverterAddress = inverterAddress;
            this.sendPort = sendPort;
            this.client = new EnIPClient(this.localAddress.ToString(), this.readTimeoutMilliseconds, udpPort);
            this.client.DeviceArrival += new DeviceArrivalHandler(this.OnDeviceArrival);
            this.client.DiscoverServers(this.sendPort);
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (this.closePacket != null)
            {
                this.remoteDevice?.ForwardClose(this.closePacket);
                this.closePacket = null;
                if (this.Input != null)
                {
                    this.Input.T2OEvent -= new T2OEventHandler(this.ImplicitMessageReceived);
                }
            }
            this.remoteDevice?.Disconnect();
            this.remoteDevice = null;
            this.implicitTimer.Change(idlePollingInterval, idlePollingInterval);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive, out int length)
        {
            var isOk = false;

            List<byte> path = GetPath(classId, instanceId, attributeId);
            int Length = 0;
            int Offset = 0;
            byte[] msg = path.ToArray();
            receive = null;

            var status = this.remoteDevice?.SendUCMM_RR_Packet(msg, serviceId, data, ref Offset, ref Length, out receive);

            isOk = (status == EnIPNetworkStatus.OnLine) && (Length > 44);
            length = Length;

            return isOk;
        }

        public bool ImplicitMessageWrite(byte[] data)
        {
            var write = false;
            if (this.Output != null)
            {
                this.Output.RawData = data;
                write = true;
            }
            return write;
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

        private static byte CalcBytes(long value)
        {
            if (value == 0)
            {
                return 1;
            }
            UInt32 bitLength = 0;
            while (value > 0)
            {
                bitLength++;
                value >>= 1;
            }
            return (byte)(Math.Ceiling(bitLength * 1.0 / 8));
        }

        private static List<byte> GetPath(UInt16 Class, UInt32 Instance, UInt16? Attribute = null) // EnIPBase.cs:GetPath is wrong
        { // see Apendix C: Data Management:
            List<byte> lb = new List<byte>();
            lb.AddRange(ItemPath(CipLogicalType.ClassID, Class));
            lb.AddRange(ItemPath(CipLogicalType.InstanceID, Instance)); // instance = 0 -> class level
            if (Attribute != null)
            {
                lb.AddRange(ItemPath(CipLogicalType.AttributeId, Attribute.Value));
            }
            return lb;
        }

        private static List<byte> ItemPath(CipLogicalType lt, UInt32 value)
        {
            List<byte> lb = new List<byte>();
            byte temp = CalcBytes(value); // maximal 32 bytes = 4 -> UInt32 value
            lb.Add((byte)(((byte)CipSegmentTypes.LogicalSegment) | ((byte)(((byte)lt) | ((byte)(temp / 2)))))); // 1,2,4 => 0,1,2   or  LogicalType  or LogicalSegment
            if (temp > 1) // padbyte
            {
                lb.Add(0);
            }
            byte[] xy = BitConverter.GetBytes(value);
            for (int i = 0; i < temp; i++) // add possible smallest representation
            {
                lb.Add(xy[i]);
            }
            return lb;
        }

        // this always arrives with a 50ms interval - even if ImplicitTimer is slower
        private void ImplicitMessageReceived(EnIPAttribut sender)
        {
            this.receiveBuffer = sender.RawData;
            var args = new ImplicitReceivedEventArgs();
            args.receivedMessage = sender.RawData;
            args.isOk = true;
            this.receivedImplicitTime = DateTime.UtcNow;
            this.ImplicitReceivedChanged?.Invoke(this, args);
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer?.Change(-1, -1);
            var isOk = false;
            try
            {
                if (this.Output != null)
                {
                    this.Output.Class1UpdateO2T();
                }
                isOk = DateTimeOffset.UtcNow.Subtract(this.receivedImplicitTime).TotalMilliseconds < this.readTimeoutMilliseconds + 500;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message);
            }

            if (!isOk || isOk != this.IsConnectedUdp || this.remoteDevice is null)
            {
                this.IsConnectedUdp = isOk;
                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.IsConnected, this.IsConnectedUdp));
            }

            if (!isOk)
            {
                this.implicitTimer?.Change(idlePollingInterval, idlePollingInterval);
            }
            else
            {
                this.implicitTimer?.Change(UdpPollingInterval, UdpPollingInterval);
            }
        }

        private void OnDeviceArrival(EnIPRemoteDevice device)
        {
            this.remoteDevice = device;
            try
            {
                if (this.remoteDevice.Connect())
                {
                    this.remoteDevice?.GetObjectList();
                    this.StartImplicitMessages();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message);
            }
        }

        private bool StartImplicitMessages()
        {
            var ipClass = this.remoteDevice?.SupportedClassLists.FirstOrDefault(c => c.Id == (ushort)CIPObjectLibrary.Assembly);
            if (ipClass is null)
            {
                throw new InverterDriverException(
                    "Inverter connection - Missing class list",
                    InverterDriverExceptionCode.TcpInverterConnectionFailed);
            }
            ipClass.ReadDataFromNetwork();

            var instOut = new EnIPInstance(ipClass, 100);
            instOut.ReadDataFromNetwork();
            this.Output = new EnIPAttribut(instOut, 3);
            this.Output.ReadDataFromNetwork();

            var instIn = new EnIPInstance(ipClass, 101);
            instIn.ReadDataFromNetwork();
            this.Input = new EnIPAttribut(instIn, 3);
            this.Input.ReadDataFromNetwork();

            var localEp = new IPEndPoint(this.localAddress, udpPort);
            this.remoteDevice?.Class1Activate(localEp);

            var status = this.remoteDevice?.ForwardOpen(null, this.Output, this.Input, out this.closePacket, (uint)this.readTimeoutMilliseconds * 1000, true, false);
            if (status == EnIPNetworkStatus.OnLine)
            {
                this.receivedImplicitTime = DateTime.UtcNow;
                if (this.Input != null)
                {
                    this.Input.T2OEvent += new T2OEventHandler(this.ImplicitMessageReceived);
                }
            }
            return status == EnIPNetworkStatus.OnLine;
        }

        #endregion
    }
}
