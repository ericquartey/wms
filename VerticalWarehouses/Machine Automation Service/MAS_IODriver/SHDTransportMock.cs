using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.IODriver.Interface;

namespace Ferretto.VW.MAS.IODriver
{
    //TEMP The TransportMock handle only data for fictitious device with firmware release 0x10
    public class SHDTransportMock : ISHDTransport
    {
        #region Fields

        private const byte FW_RELEASE = 0x10;

        private const int NBYTES_RECEIVE = 15;

        private const int NBYTES_RECEIVE_CFG = 3;

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
        public bool IsReadingOk { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(IPAddress ioAddress, int sendPort)
        {
        }

        /// <inheritdoc />
        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
        }

        /// <inheritdoc />
        public async ValueTask<byte[]> ReadAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(3, stoppingToken);

            this.IsReadingOk = true;
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

        /// <inheritdoc />
        public async ValueTask<int> WriteAsync(byte[] dataMessage, CancellationToken stoppingToken)
        {
            lock (this.responseMessage)
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

        private byte BoolArrayToByte(bool[] b)
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
            for (var i = 0; i < 8; i++)
            {
                outputs[i] = false;
            }
            rawMessage[4] = this.BoolArrayToByte(outputs);

            // Payload input
            var lowByteInputs = new bool[8];
            var highByteInputs = new bool[8];
            for (var i = 0; i < 8; i++)
            {
                lowByteInputs[i] = false;
                highByteInputs[i] = false;
            }
            rawMessage[5] = this.BoolArrayToByte(lowByteInputs);
            rawMessage[6] = this.BoolArrayToByte(highByteInputs);

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
            var relProtocol = inputTelegram[1];
            var codeOperation = inputTelegram[2];

            switch (codeOperation)
            {
                case 0x00: // Data
                    this.responseMessage[0] = NBYTES_RECEIVE;  // nBytes
                    this.responseMessage[1] = FW_RELEASE;      // fwRelease
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
                    this.responseMessage[5] = this.BoolArrayToByte(lowByteInputs);
                    this.responseMessage[6] = this.BoolArrayToByte(highByteInputs);

                    // configuration
                    for (var i = 0; i < 8; i++)
                    {
                        this.responseMessage[7 + i] = 0x00;
                    }

                    break;

                case 0x01: // Config
                    this.responseMessage[0] = NBYTES_RECEIVE_CFG;  // nBytes
                    this.responseMessage[1] = FW_RELEASE;          // fwRelease
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
