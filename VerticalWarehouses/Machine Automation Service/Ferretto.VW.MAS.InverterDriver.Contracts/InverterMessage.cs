using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public sealed class InverterMessage
    {
        #region Fields

        private const byte ACTUAL_DATA_SET_INDEX = 0x05; //VALUE fixed data set id used for the inverter operation

        private const byte READ_HEADER = 0x00;

        private const byte WRITE_HEADER = 0x80;

        private readonly Dictionary<int, string> telegramErrorText = new Dictionary<int, string>()
        {
            {0, "no error" },
            {1, "Non-permissible parameter value"},
            {2, "Non-permissible dataset"},
            {3, "Parameter not readable (write-only)"},
            {4, "Parameter not writable (read-only)"},
            {5, "EEPROM read error"},
            {6, "EEPROM write error"},
            {7, "EEPROM checksum error"},
            {8, "Parameter cannot be written while drive is running"},
            {9, "Values of data sets are different"},
            {10, "Not available"},
            {11, "Unknown parameter"},
            {12, "Not available"},
            {13, "Syntax error in received telegram"},
            {14, "Data type of parameter does not correspond to the number of bytes in the telegram"},
            {15, "Unknown error"},
            {20, "Selected System Bus node not available"},
            {30, "Syntax error in received telegram"},
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

        public InverterMessage(byte[] rawMessage)
        {
            this.responseMessage = true;

            this.heartbeatMessage = false;

            this.sendDelay = 0;

            if (rawMessage[1] == 0x00)
            {
                throw new InverterDriverException("Invalid raw data");
            }

            this.IsWriteMessage = (rawMessage[0] & 0x80) > 0;

            this.IsError = (rawMessage[0] & 0x40) > 0;

            this.payloadLength = rawMessage[1] - 4;

            this.SystemIndex = rawMessage[2];

            this.DataSetIndex = rawMessage[3];

            //VALUE parameterId is always stored starting at byte index 4 in the byte array
            this.parameterId = BitConverter.ToInt16(rawMessage, 4);

            this.payload = new byte[this.payloadLength];
            try
            {
                Array.Copy(rawMessage, 6, this.payload, 0, this.payloadLength);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while parsing raw message bytes\nLength:{rawMessage.Length} payloadLength:{this.payloadLength}", InverterDriverExceptionCode.InverterPacketMalformed, ex);
            }
        }

        public InverterMessage(InverterIndex systemIndex, InverterParameterId parameterId)
            : this((short)systemIndex, parameterId)
        {
        }

        public InverterMessage(short systemIndex, InverterParameterId parameterId)
        {
            this.responseMessage = false;
            this.SystemIndex = (byte)systemIndex;
            this.DataSetIndex = ACTUAL_DATA_SET_INDEX;
            this.parameterId = (short)parameterId;
            this.IsWriteMessage = false;
            this.heartbeatMessage = false;
        }

        public InverterMessage(byte systemIndex, short parameterId, object payload, InverterDataset dataSetIndex = InverterDataset.ActualDataset, int sendDelay = 0)
        {
            this.BuildWriteMessage(systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        public InverterMessage(InverterIndex systemIndex, short parameterId, object payload, InverterDataset dataSetIndex = InverterDataset.ActualDataset, int sendDelay = 0)
        {
            this.BuildWriteMessage((byte)systemIndex, parameterId, payload, (byte)dataSetIndex, sendDelay);
        }

        #endregion

        #region Properties

        public byte DataSetIndex { get; private set; }

        public bool HeartbeatValue => (this.ConvertPayloadToUShort() & 0x0400) > 0;

        public int IntPayload => this.ConvertPayloadToInt();

        public bool IsError { get; }

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

        public string StringPayload => this.ConvertPayloadToString();

        public byte SystemIndex { get; private set; }

        public string TelegramErrorText
        {
            get
            {
                return this.telegramErrorText.ContainsKey(this.UShortPayload) ? this.telegramErrorText[this.UShortPayload] : string.Empty;
            }
        }

        public uint UIntPayload => this.ConvertPayloadToUInt();

        public ushort UShortPayload => this.ConvertPayloadToUShort();

        #endregion

        #region Methods

        public static InverterMessage FromBytes(byte[] messageBytes)
        {
            return new InverterMessage(messageBytes);
        }

        public byte[] GetHeartbeatMessage(bool setBit)
        {
            if (this.parameterId != (short)InverterParameterId.ControlWordParam)
            {
                throw new InverterDriverException("Invalid parameter id");
            }

            this.heartbeatMessage = true;

            //VALUE 10th byte of control word value represents Heartbeat flag
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
        ///     Returns a byte array from the current Inverter Message ready to be sent to the Inverter hardware
        /// </summary>
        /// <returns>Byte array containing the request message for the inverter</returns>
        /// <exception cref="InverterDriverException">Configured ParameterId failed to convert into byte array</exception>
        public byte[] GetReadMessage()
        {
            var readMessage = new byte[6];

            readMessage[0] = READ_HEADER;
            readMessage[1] = 0x04; //VALUE Fixed packed data length to 4 bytes
            readMessage[2] = this.SystemIndex;
            readMessage[3] = this.DataSetIndex;

            if (this.parameterId.Equals(InverterParameterId.ControlWordParam) || this.IsWriteMessage)
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

            writeMessage[0] = WRITE_HEADER;
            writeMessage[1] =
                Convert.ToByte(messageLength - 2); //VALUE Data length does not include header and length bytes
            writeMessage[2] = this.SystemIndex;
            writeMessage[3] = this.DataSetIndex;

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

            returnString.Append("InverterMessage:");

            returnString.Append($"SystemIndex={this.SystemIndex}:");

            returnString.Append($"IsWriteMessage={this.IsWriteMessage}:");

            returnString.Append($"parameterId={this.parameterId}:");

            returnString.Append($"DataSetIndex={this.DataSetIndex}:");

            returnString.Append($"payloadLength={this.payloadLength:X}:");

            returnString.Append($"payload=");
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

        private void BuildWriteMessage(byte systemIndex, short parameterId, object payload, byte dataSetIndex, int sendDelay = 0)
        {
            this.responseMessage = false;
            this.SystemIndex = systemIndex;
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
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusDigitalSignals:
                    if (this.payloadLength == 2)
                    {
                        returnValue = BitConverter.ToUInt16(this.payload, 0);
                    }

                    break;

                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.ActualPositionShaft:
                    if (this.payloadLength == 4)
                    {
                        returnValue = BitConverter.ToInt32(this.payload, 0);
                    }

                    break;

                case InverterParameterId.DigitalInputsOutputs:
                    returnValue = Encoding.ASCII.GetString(this.payload);
                    break;
            }

            return returnValue;
        }

        private int ConvertPayloadToInt()
        {
            var returnValue = default(int);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
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
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.TorqueCurrent:
                case InverterParameterId.TableTravelTableIndex:
                case InverterParameterId.TableTravelDirection:
                case InverterParameterId.ShutterTargetPosition:
                case InverterParameterId.HomingCalibration:
                    if (this.payloadLength == 2)
                    {
                        returnValue = BitConverter.ToUInt16(this.payload, 0);
                    }
                    break;

                default:
                    returnValue = default(ushort);
                    break;
            }

            return returnValue;
        }

        private void SetPayload(object payload)
        {
            if (payload is null)
            {
                this.payload = new byte[0];
                this.payloadLength = 0;
                return;
            }

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
                    if (Debugger.IsAttached) Debugger.Break();
                    this.payload = new byte[0];
                    this.payloadLength = 0;
                    return;
            }

            this.payloadLength = this.payload.Length;
        }

        #endregion
    }
}
