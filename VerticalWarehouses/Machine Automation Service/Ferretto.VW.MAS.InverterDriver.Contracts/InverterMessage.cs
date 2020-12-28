using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public sealed class InverterMessage
    {
        #region Fields

        private const byte ActualDataSetIndex = 0x05; // VALUE fixed data set id used for the inverter operation

        private const int DatasetByteIndex = 3;

        private const byte ReadHeader = 0x00;

        private const int SystemIndexByteIndex = 2;

        private const byte WriteHeader = 0x80;

        private readonly Dictionary<int, string> telegramErrorText = new Dictionary<int, string>()
        {
            { 0, " no error" },
            { 1, " Non-permissible parameter value" },
            { 2, " Non-permissible dataset" },
            { 3, " Parameter not readable (write-only)" },
            { 4, " Parameter not writable (read-only)" },
            { 5, " EEPROM read error" },
            { 6, " EEPROM write error" },
            { 7, " EEPROM checksum error" },
            { 8, " Parameter cannot be written while drive is running" },
            { 9, " Values of data sets are different" },
            { 10, " Not available" },
            { 11, " Unknown parameter" },
            { 12, " Not available" },
            { 13, " Syntax error in received telegram" },
            { 14, " Data type of parameter does not correspond to the number of bytes in the telegram" },
            { 15, " Unknown error" },
            { 20, " Selected System Bus node not available" },
            { 30, " Syntax error in received telegram" },
        };

        private bool heartbeatMessage;

        private short parameterId;

        private byte[] payload;

        private int payloadLength;

        private bool responseMessage;

        private int sendDelay;

        #endregion

        #region Constructors

        public InverterMessage(InverterMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.IsWriteMessage = message.IsWriteMessage;
            this.SystemIndex = message.SystemIndex;
            this.DataSetIndex = message.DataSetIndex;
            this.IsError = message.IsError;
            this.parameterId = message.parameterId;
            this.heartbeatMessage = message.heartbeatMessage;
            this.responseMessage = message.responseMessage;
            this.payload = new byte[message.payload.Length];
            this.payloadLength = message.payloadLength;
            this.sendDelay = message.sendDelay;

            try
            {
                Array.Copy(message.payload, this.payload, message.payload.Length);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException(
                    $"Exception {ex.Message} while copying payload in Copy Constructor",
                    ex);
            }
        }

        public InverterMessage(InverterIndex systemIndex, InverterParameterId parameterId, InverterDataset dataSetIndex = InverterDataset.ActualDataset)
            : this((short)systemIndex, parameterId, dataSetIndex)
        {
        }

        public InverterMessage(short systemIndex, InverterParameterId parameterId, InverterDataset dataSetIndex = InverterDataset.ActualDataset)
        {
            if (!Enum.TryParse(systemIndex.ToString(), out InverterIndex inverterIndex))
            {
                throw new ArgumentException($"8:Invalid system index {systemIndex}", nameof(systemIndex));
            }

            this.responseMessage = false;
            this.SystemIndex = inverterIndex;
            this.DataSetIndex = (byte)dataSetIndex;
            this.parameterId = (short)parameterId;
            this.IsWriteMessage = false;
            this.heartbeatMessage = false;
        }

        public InverterMessage(short systemIndex, InverterParameterId parameterId, int dataSetIndex)
        {
            if (!Enum.TryParse(systemIndex.ToString(), out InverterIndex inverterIndex))
            {
                throw new ArgumentException($"8:Invalid system index {systemIndex}", nameof(systemIndex));
            }

            this.responseMessage = false;
            this.SystemIndex = inverterIndex;
            this.DataSetIndex = (byte)dataSetIndex;
            this.parameterId = (short)parameterId;
            this.IsWriteMessage = false;
            this.heartbeatMessage = false;
        }

        public InverterMessage(short systemIndex, short parameterId, int dataSetIndex)
        {
            if (!Enum.TryParse(systemIndex.ToString(), out InverterIndex inverterIndex))
            {
                throw new ArgumentException($"8:Invalid system index {systemIndex}", nameof(systemIndex));
            }

            this.responseMessage = false;
            this.SystemIndex = inverterIndex;
            this.DataSetIndex = (byte)dataSetIndex;
            this.parameterId = (short)parameterId;
            this.IsWriteMessage = false;
            this.heartbeatMessage = false;
        }

        public InverterMessage(byte systemIndex, short parameterId, object payload, byte dataSetIndex, int sendDelay = 0)
        {
            this.BuildWriteMessage(systemIndex, parameterId, payload, dataSetIndex, sendDelay);
        }

        public InverterMessage(byte systemIndex, short parameterId, object payload, InverterDataset dataSetIndex = InverterDataset.ActualDataset, int sendDelay = 0)
        {
            this.BuildWriteMessage(systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        public InverterMessage(byte systemIndex, short parameterId, object payload, int dataSetIndex, int sendDelay = 0)
        {
            this.BuildWriteMessage(systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        public InverterMessage(InverterIndex systemIndex, short parameterId, object payload, InverterDataset dataSetIndex = InverterDataset.ActualDataset, int sendDelay = 0)
        {
            this.BuildWriteMessage((byte)systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        public InverterMessage(InverterIndex systemIndex, short parameterId, object payload, int dataSetIndex, int sendDelay = 0)
        {
            this.BuildWriteMessage((byte)systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        private InverterMessage()
        {
        }

        #endregion

        #region Properties

        public byte DataSetIndex { get; private set; }

        public bool HeartbeatValue => (this.ConvertPayloadToUShort() & 0x0400) > 0;

        public int IntPayload => this.ConvertPayloadToInt();

        public bool IsError { get; private set; }

        public bool IsHeartbeatMessage => this.heartbeatMessage;

        public bool IsReadMessage => !this.IsWriteMessage;

        public bool IsResponseMessage => this.responseMessage;

        public bool IsWriteMessage { get; private set; }

        public InverterParameterId ParameterId => (InverterParameterId)this.parameterId;

        public object Payload
        {
            get => this.ConvertPayload();
            set => this.SetPayload(value);
        }

        public int SendDelay => this.sendDelay;

        public short ShortParameterId => this.parameterId;

        public string StringPayload => this.ConvertPayloadToString();

        public InverterIndex SystemIndex
        { get; private set; }

        public string TelegramErrorText => this.telegramErrorText.ContainsKey(this.UShortPayload)
            ? this.UShortPayload.ToString() + this.telegramErrorText[this.UShortPayload]
            : this.UShortPayload.ToString();

        public uint UIntPayload => this.ConvertPayloadToUInt();

        public ushort UShortPayload => this.ConvertPayloadToUShort();

        #endregion

        #region Methods

        public static string FormatBlockWrite(object[] blockValues)
        {
            if (blockValues is null)
            {
                throw new ArgumentNullException(nameof(blockValues));
            }

            var text = new StringBuilder();
            foreach (var block in blockValues)
            {
                switch (block.GetType().Name)
                {
                    case "Int16":
                    case "UInt16":
                        text.AppendFormat("{0:X4}", (short)block);
                        break;

                    case "Int32":
                    case "UInt32":
                        text.AppendFormat("{0:X8}", (int)block);
                        break;

                    default:
                        throw new InverterDriverException($"Block write parameter {block.GetType().Name} not valid");
                }
            }

            if (text.Length > 80)
            {
                throw new InverterDriverException("Too many parameters for one block message");
            }

            return text.ToString();
        }

        public static InverterMessage FromBytes(byte[] messageBytes)
        {
            if (messageBytes is null)
            {
                throw new ArgumentNullException(nameof(messageBytes));
            }

            var message = new InverterMessage();

            message.responseMessage = true;

            message.heartbeatMessage = false;

            message.sendDelay = 0;

            if (messageBytes[1] == 0x00)
            {
                throw new InverterDriverException("Invalid raw data");
            }

            message.IsWriteMessage = (messageBytes[0] & 0x80) > 0;

            message.IsError = (messageBytes[0] & 0x40) > 0;

            message.payloadLength = messageBytes[1] - 4;

            var systemIndex = messageBytes[SystemIndexByteIndex];

            if (!Enum.TryParse(systemIndex.ToString(), out InverterIndex inverterIndex))
            {
                throw new ArgumentException(
                    $"Invalid inverter message: system index {systemIndex} is out of range [{BitConverter.ToString(messageBytes)}]",
                    nameof(messageBytes));
            }

            message.SystemIndex = inverterIndex;

            message.DataSetIndex = messageBytes[DatasetByteIndex];

            // VALUE parameterId is always stored starting at byte index 4 in the byte array
            message.parameterId = BitConverter.ToInt16(messageBytes, 4);

            message.payload = new byte[message.payloadLength];

            try
            {
                Array.Copy(messageBytes, 6, message.payload, 0, message.payloadLength);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException(
                    $"Invalid inverter message Exception {ex.Message} while parsing raw message bytes\nLength:{messageBytes.Length} payloadLength:{message.payloadLength}",
                    InverterDriverExceptionCode.InverterPacketMalformed,
                    ex);
            }

            return message;
        }

        public object[] ConvertPayloadToBlockRead(List<InverterBlockDefinition> blockDefinitions)
        {
            if (blockDefinitions is null)
            {
                throw new ArgumentNullException(nameof(blockDefinitions));
            }

            var blockValues = new object[blockDefinitions.Count];
            var stringPayload = Encoding.ASCII.GetString(this.payload);
            var start = 0;
            for (var iblock = 0; iblock < blockValues.Length; iblock++)
            {
                int length;
                switch (blockDefinitions[iblock].ParameterId)
                {
                    case InverterParameterId.TableTravelTableIndex:
                        length = 4;
                        break;

                    case InverterParameterId.TableTravelTargetSpeeds:
                    case InverterParameterId.TableTravelTargetAccelerations:
                    case InverterParameterId.TableTravelTargetDecelerations:
                    case InverterParameterId.TableTravelTargetPosition:
                    case InverterParameterId.PositionTargetPosition:
                    case InverterParameterId.PositionTargetSpeed:
                    case InverterParameterId.PositionAcceleration:
                    case InverterParameterId.PositionDeceleration:
                        length = 8;
                        break;

                    default:
                        throw new InverterDriverException($"Block read parameter {blockValues[iblock].GetType().Name} not valid");
                }

                if (start + length > stringPayload.Length)
                {
                    throw new InverterDriverException("Received too short block message");
                }

                blockValues[iblock] = Convert.ToInt32(stringPayload.Substring(start, length), 16);
                start += length;
            }

            return blockValues;
        }

        public byte[] GetHeartbeatMessage(bool setBit)
        {
            if (this.parameterId != (short)InverterParameterId.ControlWord)
            {
                throw new InverterDriverException("Invalid parameter id");
            }

            this.heartbeatMessage = true;

            // VALUE 10th byte of control word value represents Heartbeat flag
            if (setBit)
            {
                this.payload[1] |= 0x04;
            }
            else
            {
                this.payload[1] &= 0xFB;
            }

            return this.ToBytes();
        }

        /// <summary>
        ///     Returns a byte array from the current Inverter Message ready to be sent to the Inverter hardware.
        /// </summary>
        /// <returns>Byte array containing the request message for the inverter.</returns>
        /// <exception cref="InverterDriverException">Configured ParameterId failed to convert into byte array.</exception>
        public byte[] GetReadMessage()
        {
            var readMessage = new byte[6];

            readMessage[0] = ReadHeader;
            readMessage[1] = 0x04; // VALUE Fixed packed data length to 4 bytes
            readMessage[SystemIndexByteIndex] = (byte)this.SystemIndex;
            readMessage[DatasetByteIndex] = this.DataSetIndex;

            if (this.parameterId.Equals(InverterParameterId.ControlWord) || this.IsWriteMessage)
            {
                throw new InverterDriverException("Invalid Operation", InverterDriverExceptionCode.RequestReadOnWriteOnlyParameter);
            }

            var parameterIdBytes = BitConverter.GetBytes(this.parameterId);

            try
            {
                Array.Copy(parameterIdBytes, 0, readMessage, 4, parameterIdBytes.Length);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while converting ParameterId to bytes", ex);
            }

            return readMessage;
        }

        public byte[] ToBytes()
        {
            var messageLength = this.payload.Length + 6;
            var writeMessage = new byte[messageLength];

            writeMessage[0] = WriteHeader;
            writeMessage[1] =
                Convert.ToByte(messageLength - 2); // VALUE Data length does not include header and length bytes
            writeMessage[SystemIndexByteIndex] = (byte)this.SystemIndex;
            writeMessage[DatasetByteIndex] = this.DataSetIndex;

            var parameterIdBytes = BitConverter.GetBytes(this.parameterId);

            try
            {
                Array.Copy(parameterIdBytes, 0, writeMessage, 4, parameterIdBytes.Length);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while converting ParameterId to bytes", ex);
            }

            try
            {
                Array.Copy(this.payload, 0, writeMessage, 6, this.payload.Length);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException(
                    $"Exception {ex.Message} while copying payload bytes to final message", ex);
            }

            return writeMessage;
        }

        public override string ToString()
        {
            var returnString = new StringBuilder();

            returnString
                .Append("InverterMessage:")
                .Append($"SystemIndex={(byte)this.SystemIndex}:")
                .Append($"IsWriteMessage={this.IsWriteMessage}:")
                .Append($"parameterId={this.parameterId}:")
                .Append($"DataSetIndex={this.DataSetIndex}:")
                .Append($"payloadLength={this.payloadLength:X}:")
                .Append($"payload=");

            if (this.payload != null)
            {
                if ((InverterParameterId)this.parameterId == InverterParameterId.DigitalInputsOutputs)
                {
                    returnString.Append($"{this.StringPayload}");
                }
                else
                {
                    returnString.Append(" 0x ");
                    var sb = new StringBuilder();
                    foreach (var b in this.payload)
                    {
                        sb.AppendFormat("{0:x2};", b);
                    }

                    returnString.Append($"{sb}");
                }
            }
            else
            {
                returnString.Append($" null");
            }

            if (this.IsError)
            {
                returnString.Append($":Error={this.TelegramErrorText}");
            }

            return returnString.ToString();
        }

        private static string FormatBlockDefinition(List<InverterBlockDefinition> blockDefinitions)
        {
            var text = new StringBuilder();
            foreach (var block in blockDefinitions)
            {
                text.AppendFormat("{0}", (byte)block.SystemIndex);
                text.AppendFormat("{0}", (byte)block.DataSetIndex);
                text.AppendFormat("{0:X1}", (short)block.ParameterId / 100);
                text.AppendFormat("{0:00}", (short)block.ParameterId % 100);
            }

            if (text.Length > 80)
            {
                throw new InverterDriverException("Too many parameters for one block message");
            }

            return text.ToString();
        }

        private void BuildWriteMessage(byte systemIndex, short parameterId, object payload, byte dataSetIndex, int sendDelay = 0)
        {
            if (!Enum.TryParse(systemIndex.ToString(), out InverterIndex inverterIndex))
            {
                throw new ArgumentException($"8:Invalid system index {systemIndex}", nameof(systemIndex));
            }

            this.responseMessage = false;
            this.SystemIndex = inverterIndex;
            this.DataSetIndex = dataSetIndex;
            this.parameterId = parameterId;
            this.IsWriteMessage = true;
            this.heartbeatMessage = false;
            this.sendDelay = sendDelay;
            this.SetPayload(payload);
        }

        private object ConvertPayload()
        {
            var returnValue = default(object);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.CurrentError:
                case InverterParameterId.ControlWord:
                case InverterParameterId.StatusWord:
                case InverterParameterId.SetOperatingMode:
                case InverterParameterId.StatusDigitalSignals:
                    if (this.payloadLength == 2)
                    {
                        returnValue = BitConverter.ToUInt16(this.payload, 0);
                    }

                    break;

                case InverterParameterId.HomingCreepSpeed:
                case InverterParameterId.HomingFastSpeed:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAcceleration:
                case InverterParameterId.PositionDeceleration:
                case InverterParameterId.PositionTargetPosition:
                case InverterParameterId.PositionTargetSpeed:
                case InverterParameterId.ActualPositionShaft:
                    if (this.payloadLength == 4)
                    {
                        returnValue = BitConverter.ToInt32(this.payload, 0);
                    }

                    break;

                case InverterParameterId.DigitalInputsOutputs:
                    returnValue = Encoding.ASCII.GetString(this.payload);
                    break;

                case InverterParameterId.BlockDefinition:
                    returnValue = this.ConvertPayloadToBlockDefinitions();
                    break;
            }

            return returnValue;
        }

        private List<InverterBlockDefinition> ConvertPayloadToBlockDefinitions()
        {
            var definitions = new List<InverterBlockDefinition>();
            var stringPayload = Encoding.ASCII.GetString(this.payload);
            var start = 0;
            for (var iblock = 0; (start + 5) <= stringPayload.Length; iblock++)
            {
                try
                {
                    var systemIndex = int.Parse(stringPayload.Substring(start, 1));
                    var dataset = int.Parse(stringPayload.Substring(start + 1, 1));
                    var parameterId1 = Convert.ToInt32(stringPayload.Substring(start + 2, 1), 16);
                    var parameterId2 = int.Parse(stringPayload.Substring(start + 3, 2));
                    definitions.Add(new InverterBlockDefinition((InverterIndex)systemIndex, (InverterParameterId)((parameterId1 * 100) + parameterId2), (InverterDataset)dataset));
                }
                catch (Exception ex)
                {
                    throw new InverterDriverException("Received not valid block definition message", ex);
                }

                start += 5;
            }

            return definitions;
        }

        private int ConvertPayloadToInt()
        {
            var returnValue = default(int);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.HomingCreepSpeed:
                case InverterParameterId.HomingFastSpeed:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAcceleration:
                case InverterParameterId.PositionDeceleration:
                case InverterParameterId.PositionTargetPosition:
                case InverterParameterId.PositionTargetSpeed:
                case InverterParameterId.ActualPositionShaft:
                    if (this.payloadLength == 4)
                    {
                        returnValue = BitConverter.ToInt32(this.payload, 0);
                    }

                    break;
            }

            return returnValue;
        }

        private string ConvertPayloadToString()
        {
            var returnValue = default(string);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.DigitalInputsOutputs:
                    returnValue = Encoding.ASCII.GetString(this.payload);
                    break;
            }

            return returnValue;
        }

        private uint ConvertPayloadToUInt()
        {
            if (this.payloadLength != 4)
            {
                throw new Exception();
            }

            return BitConverter.ToUInt32(this.payload, 0);
        }

        private ushort ConvertPayloadToUShort()
        {
            var returnValue = default(ushort);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.ControlWord:
                case InverterParameterId.StatusWord:
                case InverterParameterId.SetOperatingMode:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.TorqueCurrent:
                case InverterParameterId.TableTravelTableIndex:
                case InverterParameterId.TableTravelDirection:
                case InverterParameterId.ShutterTargetPosition:
                case InverterParameterId.HomingCalibration:
                case InverterParameterId.ProfileInput:
                case InverterParameterId.CurrentError:
                case InverterParameterId.AxisChanged:
                case InverterParameterId.ActiveDataset:
                    if (this.payloadLength == 2)
                    {
                        returnValue = BitConverter.ToUInt16(this.payload, 0);
                    }

                    break;

                default:
                    returnValue = default;
                    break;
            }

            return returnValue;
        }

        private void SetPayload(object payload)
        {
            if (payload is null)
            {
                this.payload = Array.Empty<byte>();
                this.payloadLength = 0;
                return;
            }

            if (payload is List<InverterBlockDefinition> blockDefinitions)
            {
                this.payload = Encoding.ASCII.GetBytes(FormatBlockDefinition(blockDefinitions));
            }
            else if (payload is object[] blockValues)
            {
                this.payload = Encoding.ASCII.GetBytes(FormatBlockWrite(blockValues));
            }
            else
            {
                var payloadType = payload.GetType();

                switch (payloadType.Name)
                {
                    case "Byte":
                        this.payload = new[] { (byte)payload };
                        break;

                    case "Int16":
                        this.payload = BitConverter.GetBytes((short)payload);
                        break;

                    case "UInt16":
                        this.payload = BitConverter.GetBytes((ushort)payload);
                        break;

                    case "Int32":
                        this.payload = BitConverter.GetBytes((int)payload);
                        break;

                    case "Single":
                        this.payload = BitConverter.GetBytes((float)payload);
                        break;

                    case "Double":
                        this.payload = BitConverter.GetBytes((double)payload);
                        break;

                    case "String":
                        this.payload = Encoding.ASCII.GetBytes((string)payload);
                        break;

                    default:
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }

                        this.payload = Array.Empty<byte>();
                        this.payloadLength = 0;
                        return;
                }
            }

            this.payloadLength = this.payload.Length;
        }

        #endregion
    }
}
