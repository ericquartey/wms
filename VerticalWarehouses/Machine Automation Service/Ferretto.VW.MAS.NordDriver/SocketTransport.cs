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

        /// <summary>
        /// The timeout for read operations on the socket.
        /// </summary>
        /// <remarks>
        /// The value -1 is used to indicate no timeout.
        /// </remarks>
        private readonly int readTimeoutMilliseconds;

        private readonly byte[] receiveBuffer = new byte[1024];

        private IPAddress inverterAddress;

        private bool isDisposed;

        private EnIPRemoteDevice remoteDevice;

        private int sendPort;

        private NetworkStream transportStream;

        #endregion

        #region Constructors

        public SocketTransport(IConfiguration configuration)
        {
            this.readTimeoutMilliseconds = configuration.GetValue<int>("Vertimag:Drivers:Inverter:ReadTimeoutMilliseconds", -1);
        }

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

        public bool IsConnected => this.remoteDevice?.Connected ?? false;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.inverterAddress = inverterAddress;
            this.sendPort = sendPort;
            this.remoteDevice = new EnIPRemoteDevice(new IPEndPoint(this.inverterAddress, 0xAF12), 100);
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

        /// <inheritdoc />
        public bool ExplicitMessage(ushort classId, uint instanceId, ushort attributeId, CIPServiceCodes serviceId, byte[] data, out byte[] receive)
        {
            var isOk = false;

            List<byte> path = GetPath(classId, instanceId, attributeId);
            int Lenght = 0;
            int Offset = 0;
            byte[] msg = path.ToArray();

            this.remoteDevice.SendUCMM_RR_Packet(msg, serviceId, data, ref Offset, ref Lenght, out receive);

            isOk = Lenght > 44;

            return isOk;
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Array.Empty<byte>();
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException($"Cannot access the disposed instance of {this.GetType().Name}.");
            }

            var currentTransportStream = this.transportStream;
            if (currentTransportStream is null)
            {
                throw new InverterDriverException(
                    "Transport Stream is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!currentTransportStream.CanRead)
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

            var currentReceiveBuffer = this.receiveBuffer;
            if (currentReceiveBuffer is null)
            {
                throw new InverterDriverException(
                    "Receive buffer is null",
                    InverterDriverExceptionCode.UninitializedNetworkStream);
            }

            byte[] receivedData;
            try
            {
                if (this.transportClient.Client?.Poll(this.readTimeoutMilliseconds * 1000, SelectMode.SelectRead) ?? false)
                {
                    var readBytes = await currentTransportStream.ReadAsync(currentReceiveBuffer, 0, currentReceiveBuffer.Length, stoppingToken);

                    if (readBytes > 0)
                    {
                        receivedData = new byte[readBytes];

                        Array.Copy(currentReceiveBuffer, receivedData, readBytes);
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
                this.transportStream?.Close();
                this.transportStream?.Dispose();
                this.transportStream = null;

                this.transportClient?.Close();
                this.transportClient?.Dispose();
                this.transportClient = null;
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

        #endregion
    }
}
