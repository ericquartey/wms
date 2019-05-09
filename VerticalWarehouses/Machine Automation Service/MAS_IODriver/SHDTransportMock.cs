using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS_IODriver.Interface;

namespace Ferretto.VW.MAS_IODriver
{
    // The TransportMock handle only data for device with firmware release 0x10
    public class SHDTransportMock : ISHDTransport
    {
        #region Fields

        private const int NBYTES_RECEIVE = 15;

        private const int NBYTES_RECEIVE_CFG = 3;

        //private const int NBYTES = 12;

        private readonly ManualResetEventSlim readCompleteEventSlim;

        private readonly byte[] responseMessage;

        #endregion

        #region Constructors

        public SHDTransportMock()
        {
            this.readCompleteEventSlim = new ManualResetEventSlim(false);
            this.responseMessage = this.buildRawMessageBytes();
        }

        #endregion

        #region Properties

        public bool IsConnected => true;

        #endregion

        #region Methods

        public void Configure(IPAddress ioAddress, int sendPort)
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
            await Task.Delay(3, stoppingToken);

            if (this.readCompleteEventSlim.Wait(Timeout.Infinite, stoppingToken))
            {
                lock (this.responseMessage)
                {
                    this.readCompleteEventSlim.Reset();
                    return this.responseMessage;
                }
            }

            return null;
        }

        public async ValueTask<int> WriteAsync(byte[] dataMessage, CancellationToken stoppingToken)
        {
            switch (dataMessage[2])
            {
                case 0x00:
                    this.responseMessage[0] = NBYTES_RECEIVE;
                    // Code op
                    this.responseMessage[2] = 0x00;
                    // Payload output (echo output values)
                    Array.Copy(this.responseMessage, 4, dataMessage, 3, 1);
                    // Payload inputs (create some values)
                    var lowByteInputs = new bool[8];
                    var highByteInputs = new bool[8];
                    for (var i = 0; i < 8; i++) { lowByteInputs[i] = false; highByteInputs[i] = false; }
                    this.responseMessage[5] = this.BoolArrayToByte(lowByteInputs);
                    this.responseMessage[6] = this.BoolArrayToByte(highByteInputs);
                    break;

                case 0x01:
                case 0x02:
                    this.responseMessage[0] = NBYTES_RECEIVE_CFG;
                    // Code op
                    this.responseMessage[2] = 0x06;
                    // Configuration data
                    // TODO
                    break;

                default:
                    break;
            }

            await Task.Delay(3, stoppingToken);
            this.readCompleteEventSlim.Set();

            return this.responseMessage.Length - 5;
        }

        public async ValueTask<int> WriteAsync(byte[] dataMessage, int delay, CancellationToken stoppingToken)
        {
            await Task.Delay(delay, stoppingToken);
            return await this.WriteAsync(dataMessage, stoppingToken);
        }

        private byte BoolArrayToByte(bool[] b)
        {
            const int N_BITS_8 = 8;
            var value = 0x00;
            for (var i = 0; i < N_BITS_8; i++)
            {
                value += b[i] ? 1 : 0;
            }

            return Convert.ToByte(value);
        }

        private byte[] buildRawMessageBytes()
        {
            var rawMessage = new byte[NBYTES_RECEIVE];

            // nBytes
            rawMessage[0] = NBYTES_RECEIVE;
            // Fw release
            rawMessage[1] = 0x10;
            // Code op
            rawMessage[2] = 0x00;

            // Error code
            rawMessage[3] = 0x00;

            // Payload output
            var outputs = new bool[8];
            for (var i = 0; i < 8; i++) { outputs[i] = false; }
            rawMessage[4] = this.BoolArrayToByte(outputs);

            // Payload input
            var lowByteInputs = new bool[8];
            var highByteInputs = new bool[8];
            for (var i = 0; i < 8; i++) { lowByteInputs[i] = false; highByteInputs[i] = false; }
            rawMessage[5] = this.BoolArrayToByte(lowByteInputs);
            rawMessage[6] = this.BoolArrayToByte(highByteInputs);

            // Configuration data
            var configurationData = new byte[8];
            for (var i = 0; i < 8; i++) { configurationData[i] = 0x00; }
            Array.Copy(rawMessage, 7, configurationData, 0, configurationData.Length);

            return rawMessage;
        }

        #endregion
    }
}
