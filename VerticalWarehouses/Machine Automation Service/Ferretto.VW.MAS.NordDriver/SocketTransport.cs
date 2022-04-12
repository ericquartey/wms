using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.EnIPStack;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.NordDriver
{
    public class SocketTransport : ISocketTransport, IDisposable
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

        private EnIPAttribut Input;

        private IPAddress inverterAddress;

        private bool isDisposed;

        private EnIPAttribut Output;

        private byte[] receiveBuffer = new byte[1024];

        private EnIPRemoteDevice remoteDevice;

        private int sendPort;

        #endregion

        #region Constructors

        public SocketTransport(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
            this.implicitTimer = new Timer(this.ImplicitTimer, null, -1, -1);
        }

        #endregion

        #region Events

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

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.inverterAddress = inverterAddress;
            this.sendPort = 0xAF12;
            this.remoteDevice = new EnIPRemoteDevice(new IPEndPoint(this.inverterAddress, this.sendPort), 100);
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            throw new NotImplementedException();
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

            var status = this.remoteDevice.SendUCMM_RR_Packet(msg, serviceId, data, ref Offset, ref Lenght, out receive);

            isOk = (status == EnIPNetworkStatus.OnLine) && (Lenght > 44);

            return isOk;
        }

        public bool StartImplicitMessages()
        {
            // TODO - implement correctly following variables
            ushort id = 0;
            var ipClass = new EnIPClass(this.remoteDevice, id);
            var instance = new EnIPInstance(ipClass, id);
            this.Output = new EnIPAttribut(instance, id);
            this.Input = new EnIPAttribut(instance, id);
            var status = this.remoteDevice.ForwardOpen(null, this.Output, this.Input, out var closePacket, 200 * 1000);
            if (status == EnIPNetworkStatus.OnLine)
            {
                this.implicitTimer.Change(200, 200);
                if (this.Input != null)
                {
                    this.Input.T2OEvent += new T2OEventHandler(this.ImplicitMessageReceived);
                }
            }
            return status == EnIPNetworkStatus.OnLine;
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
            if (this.receiveBuffer is null
                || this.receiveBuffer != sender.RawData)
            {
                this.receiveBuffer = sender.RawData;
                var args = new ImplicitReceivedEventArgs();
                args.receivedMessage = sender.RawData;
                this.ImplicitReceivedChanged?.Invoke(this, args);
            }
        }

        private void ImplicitTimer(object state)
        {
            this.implicitTimer.Change(-1, -1);
            if (this.Output != null)
            {
                this.Output.Class1UpdateO2T();
            }

            this.implicitTimer.Change(200, 200);
        }

        #endregion
    }
}
