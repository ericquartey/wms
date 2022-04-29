using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.NordDriver
{
    public class SocketTransportNord : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly Timer implicitTimer;

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

        private IPAddress localAddress;

        private EnIPAttribut Output;

        private byte[] receiveBuffer = new byte[1024];

        private DateTimeOffset receivedImplicitTime;

        private EnIPRemoteDevice remoteDevice;

        private int sendPort;

        #endregion

        #region Constructors

        public SocketTransportNord(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
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
        public void Configure(IPAddress inverterAddress, int sendPort, IPAddress localAddress)
        {
            this.IsConnectedUdp = false;
            this.inverterAddress = inverterAddress;
            this.localAddress = localAddress;
            this.sendPort = 0xAF12;
            this.client = new EnIPClient(localAddress.ToString(), 200);
            this.client.DeviceArrival += new DeviceArrivalHandler(this.OnDeviceArrival);
            this.client.DiscoverServers();
            this.implicitTimer.Change(1200, 1200);
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            if (!this.remoteDevice.Connect())
            {
                throw new InverterDriverException(
                    "Failed to connect to Inverter Hardware",
                    InverterDriverExceptionCode.TcpInverterConnectionFailed);
            }
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
            this.implicitTimer.Change(1200, 1200);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive)
        {
            var isOk = false;

            List<byte> path = GetPath(classId, instanceId, attributeId);
            int Lenght = 0;
            int Offset = 0;
            byte[] msg = path.ToArray();
            receive = null;

            var status = this.remoteDevice?.SendUCMM_RR_Packet(msg, serviceId, data, ref Offset, ref Lenght, out receive);

            isOk = (status == EnIPNetworkStatus.OnLine) && (Lenght > 44);

            return isOk;
        }

        public bool ImplicitMessageStart(byte[] data)
        {
            var started = false;
            if (this.Output != null
                && this.Output.RawData is null)
            {
                this.Output.RawData = data;
                started = true;
            }
            return started;
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
            this.implicitTimer.Change(-1, -1);
            if (this.Output != null)
            {
                this.Output.Class1UpdateO2T();
            }
            var isOk = DateTimeOffset.UtcNow.Subtract(this.receivedImplicitTime).TotalMilliseconds < 1500;
            if (!isOk || isOk != this.IsConnectedUdp || this.remoteDevice is null)
            {
                this.IsConnectedUdp = isOk;
                this.ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(this.IsConnected, this.IsConnectedUdp));
            }

            if (this.remoteDevice is null)
            {
                this.implicitTimer.Change(1200, 1200);
            }
            else
            {
                this.implicitTimer.Change(200, 200);
            }
        }

        private void OnDeviceArrival(EnIPRemoteDevice device)
        {
            this.remoteDevice = device;
            if (!this.remoteDevice.Connect())
            {
                throw new InverterDriverException(
                    "Failed to connect to Inverter Hardware",
                    InverterDriverExceptionCode.TcpInverterConnectionFailed);
            }
            this.remoteDevice.GetObjectList();
            this.StartImplicitMessages();
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

            var localEp = new IPEndPoint(this.localAddress, 0x8AE);
            this.remoteDevice?.Class1Activate(localEp);

            var status = this.remoteDevice?.ForwardOpen(null, this.Output, this.Input, out this.closePacket, 200 * 1000, true, false);
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
