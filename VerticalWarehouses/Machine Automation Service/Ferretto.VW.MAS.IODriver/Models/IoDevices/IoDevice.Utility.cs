using System;
using System.Collections;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ParsingDataBytes(
            byte[] telegram,
            out int nBytesReceived,
            out ShdFormatDataOperation formatDataOperation,
            out byte fwRelease,
            ref bool[] inputs,
            ref bool[] outputs,
            out byte[] configurationData,
            out byte errorCode,
            ref int[] diagOutCurrent,
            ref bool[] diagOutFault)
        {
            const int N_BYTES8 = 8;
            const int N_BITS8 = 8;
            const int N_BITS16 = 16;

            const int NBYTES_TELEGRAM_DATA = 15;
            const int NBYTES_TELEGRAM_ACK = 3;

            Array.Clear(inputs, 0, inputs.Length);
            Array.Clear(outputs, 0, outputs.Length);
            Array.Clear(diagOutCurrent, 0, diagOutCurrent.Length);
            Array.Clear(diagOutFault, 0, diagOutFault.Length);

            configurationData = null;
            formatDataOperation = ShdFormatDataOperation.Data;
            fwRelease = 0x00;
            errorCode = 0x00;
            nBytesReceived = 0;

            if (telegram == null)
            {
                return;
            }

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

                                // Error code
                                errorCode = telegram[3];

                                // Payload output
                                var payloadOutput = telegram[4];

                                outputs = new bool[N_BITS8];
                                Array.Copy(ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[5];

                                // Payload input (High byte)
                                var payloadInputHigh = telegram[6];

                                inputs = new bool[N_BITS16];
                                Array.Copy(ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                // Configuration data
                                configurationData = new byte[N_BYTES8];
                                Array.Copy(telegram, 7, configurationData, 0, N_BYTES8);

                                // Format data operation
                                formatDataOperation = ShdFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];

                                // Code op

                                // Format data operation
                                formatDataOperation = ShdFormatDataOperation.Ack;

                                break;
                        }

                        break;

                    case 0x11:
                    case 0x12: // new release
                    case 0x13:
                        switch (nBytesReceived)
                        {
                            case NBYTES_TELEGRAM_DATA + 11: // 26
                                                            // Fw release
                                fwRelease = telegram[1];

                                // Error code
                                errorCode = telegram[4];

                                // Payload output
                                var payloadOutput = telegram[5];
                                outputs = new bool[N_BITS8];
                                Array.Copy(ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[6];

                                // Payload input (High byte)
                                var payloadInputHigh = telegram[7];

                                inputs = new bool[N_BITS16];
                                Array.Copy(ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                diagOutCurrent = ByteArrayToIntArray(telegram, 8, N_BITS8);

                                Array.Copy(ByteArrayToBoolArray(telegram[24]), diagOutFault, N_BITS8);

                                // Configuration data
                                configurationData = new byte[17];
                                Array.Copy(telegram, 8, configurationData, 0, 17);

                                // Format data operation
                                formatDataOperation = ShdFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];

                                // Code op

                                // Format data operation
                                formatDataOperation = ShdFormatDataOperation.Ack;

                                break;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                var errorNotification = new FieldNotificationMessage(
                    null,
                    $"Exception {ex.Message} while parsing received IO raw message bytes",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterException,
                    MessageStatus.OperationError,
                    (byte)this.deviceIndex,
                    ErrorLevel.Error);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                throw new IoDriverException($"Exception: {ex.Message} ParsingDataBytes error", IoDriverExceptionCode.CreationFailure, ex);
            }
        }

        private static bool[] ByteArrayToBoolArray(byte b)
        {
            const int N_BITS8 = 8;
            var t = new BitArray(new byte[] { b });
            var bits = new bool[N_BITS8];
            t.CopyTo(bits, 0);
            return bits;
        }

        private static int[] ByteArrayToIntArray(byte[] telegram, int sourceOffset, int bytes)
        {
            var ushortArray = new int[bytes];
            for (var i = 0; i < bytes; i++)
            {
                ushortArray[i] = telegram[(i * 2) + sourceOffset]
                               + telegram[(i * 2) + sourceOffset + 1] * 256;
            }
            return ushortArray;
        }

        private bool IsHeaderValid(byte header)
        {
            return header == 3 || header == 15 || header == 26;
        }

        private bool IsMessageLengthValid(byte firmwareVersion, byte length)
        {
            return
                (firmwareVersion == 0x10 && !(length == 15 || length == 3)) // length is not valid for old release
                ||
                (firmwareVersion == 0x11 && !(length == 26 || length == 3));
        }

        #endregion
    }
}
