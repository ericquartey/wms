using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_InverterDriver.Interface;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver
{
    public class SocketTransportMock : ISocketTransport
    {
        #region Fields

        private readonly ManualResetEventSlim readCompleteEventSlim;

        private InverterMessage lastControlWordMessage;

        private byte[] responseMessage;

        #endregion

        #region Constructors

        public SocketTransportMock()
        {
            this.readCompleteEventSlim = new ManualResetEventSlim(false);
        }

        #endregion

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
            if (this.readCompleteEventSlim.Wait(Timeout.Infinite, stoppingToken))
            {
                this.readCompleteEventSlim.Reset();

                await Task.Delay(25, stoppingToken);

                return this.responseMessage;
            }
            return null;
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            return await ParseWriteMessage(inverterMessage, stoppingToken);
        }

        public async ValueTask<int> WriteAsync(byte[] inverterMessage, int delay, CancellationToken stoppingToken)
        {
            await Task.Delay(delay, stoppingToken);
            return await WriteAsync(inverterMessage, stoppingToken);
        }

        private byte[] BuildRawStatusMessage(ushort payload)
        {
            byte[] rawMessage = new byte[8];
            rawMessage[0] = 0x00;
            rawMessage[1] = 0x06;
            rawMessage[2] = 0x00;
            rawMessage[3] = 0x05;
            rawMessage[4] = 0x9B;
            rawMessage[5] = 0x01;
            byte[] payloadBytes = BitConverter.GetBytes(payload);
            rawMessage[6] = payloadBytes[0];
            rawMessage[7] = payloadBytes[1];

            return rawMessage;
        }

        private async Task<int> ParseReadMessage(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            switch (inverterMessage.ParameterId)
            {
                case InverterParameterId.StatusWordParam:
                    return await ProcessStatusWordPayload(inverterMessage, stoppingToken);
            }
            return inverterMessage.GetReadMessage().Length;
        }

        private async Task<int> ParseWriteMessage(byte[] inverterMessage, CancellationToken stoppingToken)
        {
            InverterMessage message = new InverterMessage(inverterMessage);

            if (message.IsWriteMessage)
            {
                return await this.ParseWriteMessage(message, stoppingToken);
            }

            return await this.ParseReadMessage(message, stoppingToken);
        }

        private async Task<int> ParseWriteMessage(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            switch (inverterMessage.ParameterId)
            {
                case InverterParameterId.ControlWordParam:
                    return await ProcessControlWordPayload(inverterMessage, stoppingToken);
            }

            return inverterMessage.GetWriteMessage().Length;
        }

        private async Task<int> ProcessControlWordPayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            this.responseMessage = inverterMessage.GetWriteMessage();

            this.lastControlWordMessage = inverterMessage;

            await Task.Delay(5, stoppingToken);

            this.readCompleteEventSlim.Set();

            return inverterMessage.GetWriteMessage().Length;
        }

        private async Task<int> ProcessStatusWordPayload(InverterMessage inverterMessage, CancellationToken stoppingToken)
        {
            switch (this.lastControlWordMessage.UShortPayload)
            {
                case 0x0000:
                case 0x8000:
                    this.responseMessage = BuildRawStatusMessage(0x0050);
                    break;
            }

            await Task.Delay(5, stoppingToken);

            this.readCompleteEventSlim.Set();

            return inverterMessage.GetReadMessage().Length;
        }

        #endregion
    }
}
