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
                    // return the response message
                    this.readCompleteEventSlim.Reset();
                    return this.responseMessage;
                }
            }

            return null;
        }

        public async ValueTask<int> WriteAsync(byte[] dataMessage, CancellationToken stoppingToken)
        {
            // create here the responseMessage

            switch (dataMessage[2])
            {
                case 0x00:
                    this.responseMessage[0] = NBYTES_RECEIVE;
                    // Code op
                    this.responseMessage[2] = 0x00;
                    // Payload output (echo output values)
                    Array.Copy(dataMessage, 4, this.responseMessage, 3, 1);
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

        private async ValueTask<int> XXXWriteAsync(byte[] dataMessage, CancellationToken stoppingToken)
        {
            // create here the responseMessage

            // return the codeOperation requested by the write
            var fwRelease = dataMessage[1];
            var codeOperation = dataMessage[2];

            if (fwRelease == 0x10)
            {
                switch (codeOperation)
                {
                    case 0x00:
                        this.responseMessage[0] = NBYTES_RECEIVE;  // nBytes
                        this.responseMessage[1] = fwRelease;       // fwRelease
                        this.responseMessage[2] = 0x00;            // Code op   0x00: data, 0x06: configuration
                        this.responseMessage[3] = 0x00;            // error code
                                                                   // Payload output (echo output values)
                        Array.Copy(dataMessage, 3, this.responseMessage, 4, 1);
                        // Payload inputs (create some values)
                        var lowByteInputs = new bool[8];
                        var highByteInputs = new bool[8];  // according to the selection of axis, set the feedback DI8, DI9 digital values
                        for (var i = 0; i < 8; i++) { lowByteInputs[i] = false; highByteInputs[i] = false; }
                        this.responseMessage[5] = this.BoolArrayToByte(lowByteInputs);
                        this.responseMessage[6] = this.BoolArrayToByte(highByteInputs);
                        // configuration
                        for (var i = 0; i < 8; i++) { this.responseMessage[7 + i] = 0x00; }

                        break;

                    case 0x01:
                    case 0x02:
                        this.responseMessage[0] = NBYTES_RECEIVE_CFG;  // nBytes
                        this.responseMessage[1] = fwRelease;           // fwRelease
                        this.responseMessage[2] = 0x06;                // Code op  0x00: data, 0x06: configuration

                        break;

                    default:
                        break;
                }
            }

            if (fwRelease == 0x11)
            {
                switch (codeOperation)
                {
                    case 0x00:
                        this.responseMessage[0] = NBYTES_RECEIVE;  // nBytes
                        this.responseMessage[1] = fwRelease;       // fwRelease
                        this.responseMessage[2] = 0x00;            // Code op   0x00: data, 0x06: configuration
                        this.responseMessage[3] = 0x00;            // alignment
                        this.responseMessage[4] = 0x00;            // error code
                        // Payload output (echo output values)
                        Array.Copy(dataMessage, 4, this.responseMessage, 5, 1);
                        // Payload inputs (create some values)
                        var lowByteInputs = new bool[8];
                        var highByteInputs = new bool[8];  // according to the selection of axis, set the feedback DI8, DI9 digital values
                        for (var i = 0; i < 8; i++) { lowByteInputs[i] = false; highByteInputs[i] = false; }
                        this.responseMessage[6] = this.BoolArrayToByte(lowByteInputs);
                        this.responseMessage[7] = this.BoolArrayToByte(highByteInputs);
                        // configuration
                        for (var i = 0; i < 17; i++) { this.responseMessage[8 + i] = 0x00; }

                        break;

                    case 0x01:
                    case 0x02:
                        this.responseMessage[0] = NBYTES_RECEIVE_CFG;  // nBytes
                        this.responseMessage[1] = fwRelease;           // fwRelease
                        this.responseMessage[2] = 0x06;                // Code op  0x00: data, 0x06: configuration

                        break;

                    default:
                        break;
                }
            }

            await Task.Delay(3, stoppingToken);
            this.readCompleteEventSlim.Set();

            return this.responseMessage.Length - 5;
        }

        #endregion

        /*

        inputs = null;
            outputs = null;
            configurationData = null;
            formatDataOperation = SHDFormatDataOperation.Data;
            fwRelease = 0x00;
            errorCode = 0x00;
            nBytesReceived = 0;

            if (telegram == null)
                return;

            byte codeOp = 0x00;

            // Parsing incoming telegram
            try
            {
                // N Bytes
                nBytesReceived = telegram[0];

                // Get the fw release
                fwRelease = telegram[1];
                switch (fwRelease)
                {
                    case 0x10: // old release
                        switch (nBytesReceived)
                        {
                            case NBYTES_TELEGRAM_DATA:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];
                                // Error code
                                errorCode = telegram[3];

                                // Payload output
                                var payloadOutput = telegram[4];
                                outputs = new bool[N_BITS8];
                                Array.Copy(this.ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[5];
                                // Payload input (High byte)
                                var payloadInputHigh = telegram[6];

                                inputs = new bool[N_BITS16];
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                // Configuration data
                                configurationData = new byte[N_BYTES8];
                                Array.Copy(telegram, 7, configurationData, 0, N_BYTES8);

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Ack;

                                break;

                            default:
                                //TODO throw an exception for the invalid telegram
                                break;
                        }

                        break;

                    case 0x11: // new release
                        switch (nBytesReceived)
                        {
                            case NBYTES_TELEGRAM_DATA + 10:  // 25
                                                             // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Alignment
                                var alignment = telegram[3];

                                // Error code
                                errorCode = telegram[4];

                                // Payload output
                                var payloadOutput = telegram[5];
                                outputs = new bool[N_BITS8];
                                Array.Copy(this.ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[6];
                                // Payload input (High byte)
                                var payloadInputHigh = telegram[7];

                                inputs = new bool[N_BITS16];
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                // Configuration data
                                configurationData = new byte[17];
                                Array.Copy(telegram, 8, configurationData, 0, 17);

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Ack;

                                break;

                            default:
                                //TODO throw an exception for the invalid telegram
                                break;
                        }

                        break;

                    default:
                        break;
                }
            }

        */
    }
}
