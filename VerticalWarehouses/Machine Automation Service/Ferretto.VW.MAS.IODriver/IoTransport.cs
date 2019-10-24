﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;

namespace Ferretto.VW.MAS.IODriver
{
    internal class IoTransport : IIoTransport, IDisposable
    {
        #region Fields

        private readonly int readTimeoutMilliseconds;   // -1 is no timeout

        private readonly byte[] receiveBuffer = new byte[1024];

        private bool disposed;

        private IPAddress ioAddress;

        private int sendPort;

        private TcpClient transportClient;

        private NetworkStream transportStream;

        #endregion

        #region Constructors

        public IoTransport(int readTimeoutMilliseconds)
        {
            this.readTimeoutMilliseconds = readTimeoutMilliseconds;
        }

        #endregion

        #region Properties

        public bool IsConnected => this.transportClient?.Connected ?? false;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress hostAddress, int sendPort)
        {
            this.ioAddress = hostAddress;
            this.sendPort = sendPort;
        }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            if (this.ioAddress == null)
            {
                throw new ArgumentNullException(
                    nameof(this.ioAddress),
                    $"{nameof(this.ioAddress)} can't be null");
            }

            if (this.ioAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "IODevice Address is not a valid IPV4 address",
                    nameof(this.ioAddress));
            }

            if (this.sendPort == 0)
            {
                throw new ArgumentNullException(nameof(this.sendPort), $"{nameof(this.sendPort)} can't be zero");
            }

            if (this.sendPort < 1024 || this.sendPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(this.sendPort),
                    $"{nameof(this.sendPort)} value must be between 1204 and 65535");
            }

            if (this.transportClient != null || this.transportStream != null)
            {
                // throw new IoDriverException(
                //    "Socket Transport is already open",
                //    IoDriverExceptionCode.SocketOpen);
                this.transportClient?.Dispose();
                this.transportStream?.Dispose();
                this.transportClient = null;
                this.transportStream = null;
            }

            try
            {
                this.transportClient = new TcpClient();
            }
            catch (Exception ex)
            {
                throw new IoDriverException(
                    "Failed to create Transport Socket client",
                    IoDriverExceptionCode.TcpClientCreationFailed,
                    ex);
            }

            try
            {
                await this.transportClient.ConnectAsync(this.ioAddress, this.sendPort);
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new IoDriverException(
                    "Failed to connect to IO Hardware",
                    IoDriverExceptionCode.TcpInverterConnectionFailed,
                    ex);
            }

            try
            {
                this.transportStream = this.transportClient.GetStream();
            }
            catch (Exception ex)
            {
                this.transportClient?.Dispose();
                this.transportClient = null;
                throw new IoDriverException(
                    "Failed to retrieve socket communication stream",
                    IoDriverExceptionCode.GetNetworkStreamFailed,
                    ex);
            }
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (!this.transportClient?.Connected ?? false)
            {
                return;
            }

            this.transportStream?.Close();
            this.transportClient?.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            if (this.transportStream == null)
            {
                throw new IoDriverException(
                    "Transport Stream is null",
                    IoDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanRead)
            {
                throw new IoDriverException(
                    "Transport Stream not configured for reading data",
                    IoDriverExceptionCode.MisconfiguredNetworkStream);
            }

            byte[] receivedData;
            try
            {
                if (this.transportClient.Client.Poll(this.readTimeoutMilliseconds * 1000, SelectMode.SelectRead))
                {
                    var readBytes = await this.transportStream.ReadAsync(this.receiveBuffer, 0, this.receiveBuffer.Length, stoppingToken);
                    if (readBytes > 0)
                    {
                        receivedData = new byte[readBytes];

                        Array.Copy(this.receiveBuffer, receivedData, readBytes);
                    }
                    else
                    {
                        this.Disconnect();
                        throw new IoDriverException("Error reading data from Transport Stream");
                    }
                }
                else
                {
                    this.Disconnect();
                    throw new IoDriverException("Timeout reading data from Transport Stream");
                }
            }
            catch (Exception ex)
            {
                this.Disconnect();
                throw new IoDriverException(
                    "Error reading data from Transport Stream",
                    IoDriverExceptionCode.NetworkStreamReadFailure,
                    ex);
            }

            return receivedData;
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] message, CancellationToken stoppingToken)
        {
            return await this.WriteAsync(message, 0, stoppingToken);
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] message, int delay, CancellationToken stoppingToken)
        {
            if (this.transportStream == null)
            {
                throw new IoDriverException(
                    "Transport Stream is null",
                    IoDriverExceptionCode.UninitializedNetworkStream);
            }

            if (!this.transportStream.CanWrite)
            {
                throw new IoDriverException(
                    "Transport Stream not configured for sending data",
                    IoDriverExceptionCode.MisconfiguredNetworkStream);
            }

            if (!this.IsConnected)
            {
                throw new IoDriverException("Error writing data to Transport Stream", IoDriverExceptionCode.NetworkStreamWriteFailure);
            }

            try
            {
                if (delay > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                await this.transportStream.WriteAsync(message, 0, message.Length, stoppingToken);
                return message.Length;
            }
            catch (Exception ex)
            {
                this.Disconnect();
                throw new IoDriverException("Error writing data to Transport Stream", IoDriverExceptionCode.NetworkStreamWriteFailure, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.transportStream?.Close();
                    this.transportStream?.Dispose();
                    this.transportStream = null;

                    this.transportClient?.Close();
                    this.transportClient?.Dispose();
                    this.transportClient = null;
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
