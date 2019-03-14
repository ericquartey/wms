using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.InverterDriver
{
    public class AsyncSocketTransport : ISocketTransport, IDisposable
    {
        #region Fields

        private readonly byte[] receiveBuffer = new byte[1024];

        private bool disposed;

        private IPAddress inverterAddress;

        private int sendPort;

        private Socket transportSocket;

        #endregion

        #region Destructors

        ~AsyncSocketTransport()
        {
            this.Dispose(true);
        }

        #endregion

        #region Properties

        public bool IsConnected => this.transportSocket?.Connected ?? false;

        #endregion

        #region Methods

        public void Configure(IPAddress inverterAddress, int sendPort)
        {
            this.inverterAddress = inverterAddress;
            this.sendPort = sendPort;
        }

        public Task ConnectAsync()
        {
            if (this.inverterAddress == null)
                throw new ArgumentNullException(nameof(this.inverterAddress),
                    $"{nameof(this.inverterAddress)} can't be null");

            if (this.inverterAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("Inverter Address is not a valid IPV4 address",
                    nameof(this.inverterAddress));

            if (this.sendPort == 0)
                throw new ArgumentNullException(nameof(this.sendPort), $"{nameof(this.sendPort)} can't be zero");

            if (this.sendPort < 1024 || this.sendPort > 65535)
                throw new ArgumentOutOfRangeException(nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and 65535");

            this.transportSocket = new Socket(this.inverterAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            return this.transportSocket.ConnectAsync(this.inverterAddress, this.sendPort);
        }

        public void Disconnect()
        {
            if (!this.transportSocket?.Connected ?? false)
            {
                return;
            }

            this.transportSocket?.Shutdown(SocketShutdown.Both);
            this.transportSocket?.Close();
            this.transportSocket?.Dispose();
            this.transportSocket = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            var memoryBuffer = new Memory<byte>(this.receiveBuffer);
            try
            {
                var readBytes = await this.transportSocket.ReceiveAsync(memoryBuffer, SocketFlags.None, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            return memoryBuffer.ToArray();
        }

        public ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            ValueTask<int> result;
            var memoryBuffer = new ReadOnlyMemory<byte>(inverterMessage);

            try
            {
                result = this.transportSocket.SendAsync(memoryBuffer, SocketFlags.None, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return new ValueTask<int>();
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
