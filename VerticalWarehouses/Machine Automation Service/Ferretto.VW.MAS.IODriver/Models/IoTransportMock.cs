using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.IODriver
{
    /// <summary>
    /// Handles socket data for fictitious device with firmware release 0x10.
    /// </summary>
    internal class IoTransportMock : IIoTransport
    {
        #region Fields

        private const byte FirmwareRelease = 0x10;

        private const int NBYTES_RECEIVE = 15;

        private const int NBYTES_RECEIVE_CFG = 3;

        private readonly ManualResetEventSlim readCompleteEventSlim;

        private readonly object syncRoot = new object();

        private byte[] responseMessage;

        #endregion

        #region Constructors

        public IoTransportMock()
        {
            this.readCompleteEventSlim = new ManualResetEventSlim(false);
            this.responseMessage = this.BuildRawMessageBytes();
        }

        #endregion

        #region Properties

        public bool IsConnected => true;

        public int ReadTimeout => 2000;

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task ConnectAsync(IPAddress ioAddress, int sendPort)
        {
            // do nothing
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            // do nothing
        }

        public void Dispose()
        {
            // do nothing
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(3, stoppingToken);

            if (this.readCompleteEventSlim.Wait(Timeout.Infinite, stoppingToken))
            {
                lock (this.syncRoot)
                {
                    this.readCompleteEventSlim.Reset();
                    return this.responseMessage;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] dataMessage, CancellationToken stoppingToken)
        {
            lock (this.syncRoot)
            {
                this.BuildResponseMessage(dataMessage);
            }

            await Task.Delay(3, stoppingToken);
            this.readCompleteEventSlim.Set();

            return this.responseMessage.Length - 5;
        }

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] dataMessage, int delay, CancellationToken stoppingToken)
        {
            await Task.Delay(delay, stoppingToken);
            return await this.WriteAsync(dataMessage, stoppingToken);
        }

        private static byte BoolArrayToByte(bool[] b)
        {
            byte value = 0x00;
            var index = 0;
            foreach (var el in b)
            {
                if (el)
                {
                    value |= (byte)(1 << index);
                }

                index++;
            }

            return value;
        }

        private byte[] BuildRawMessageBytes()
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
            for (var i = 0; i < 8; i++)
            {
                outputs[i] = false;
            }

            rawMessage[4] = BoolArrayToByte(outputs);

            // Payload input
            var lowByteInputs = new bool[8];
            var highByteInputs = new bool[8];
            for (var i = 0; i < 8; i++)
            {
                lowByteInputs[i] = false;
                highByteInputs[i] = false;
            }

            rawMessage[5] = BoolArrayToByte(lowByteInputs);
            rawMessage[6] = BoolArrayToByte(highByteInputs);

            // Configuration data
            var configurationData = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                configurationData[i] = 0x00;
            }

            Array.Copy(rawMessage, 7, configurationData, 0, configurationData.Length);

            return rawMessage;
        }

        private void BuildResponseMessage(byte[] inputTelegram)
        {
            var codeOperation = inputTelegram[2];

            switch (codeOperation)
            {
                case 0x00: // Data
                    this.responseMessage = new byte[NBYTES_RECEIVE];
                    this.responseMessage[0] = NBYTES_RECEIVE;  // nBytes
                    this.responseMessage[1] = FirmwareRelease; // fwRelease
                    this.responseMessage[2] = 0x00;            // Code op   0x00: data, 0x06: configuration
                    this.responseMessage[3] = 0x00;            // error code

                    // Payload output (echo output values)
                    Array.Copy(inputTelegram, 3, this.responseMessage, 4, 1);

                    // Payload inputs (create some values)
                    var lowByteInputs = new bool[8];
                    var highByteInputs = new bool[8];  // according to the selection of axis, set the feedback DI8, DI9 digital values
                    for (var i = 0; i < 8; i++)
                    {
                        lowByteInputs[i] = false;
                        highByteInputs[i] = false;
                    }

                    this.responseMessage[5] = BoolArrayToByte(lowByteInputs);
                    this.responseMessage[6] = BoolArrayToByte(highByteInputs);

                    // configuration
                    for (var i = 0; i < 8; i++)
                    {
                        this.responseMessage[7 + i] = 0x00;
                    }

                    break;

                case 0x01: // Config
                    this.responseMessage = new byte[NBYTES_RECEIVE_CFG];
                    this.responseMessage[0] = NBYTES_RECEIVE_CFG;  // nBytes
                    this.responseMessage[1] = FirmwareRelease;          // fwRelease
                    this.responseMessage[2] = 0x06;                // Ack  0x00: data, 0x06: configuration
                    break;

                case 0x02: // SetIP
                    // TODO
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
