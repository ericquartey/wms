﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class SocketTransportMock : ISocketTransport
    {
        #region Properties

        public bool IsConnected => true;

        #endregion

        #region Methods

        public void Configure(IPAddress inverterAddress, int sendPort)
        {
        }

        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
        }

        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            byte[] rawMessage = { 0x20, 0x06, 0x00, 0x05, 0x9A, 0x01, 0x01, 0x00 };
            await Task.Delay(1000, stoppingToken);
            return rawMessage;
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            await Task.Delay(500, stoppingToken);
            return 0;
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            await Task.Delay(500, stoppingToken);
            return 0;
        }

        #endregion
    }
}
