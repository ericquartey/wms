using System;
using System.Text;
using Ferretto.VW.Common_Utils.Exceptions;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class InverterMessage
    {
        #region Fields

        private const byte ActualDataSetIndex = 0x05; //VALUE fixed data set id used for the inverter operation

        private const byte ReadHeader = 0x00;

        private const byte WriteHeader = 0x80;

        private readonly short parameterId;

        private readonly byte[] payload;

        private readonly int payloadLenght;

        private readonly bool responseMessage;

        private bool heartbeatMessage;

        private byte writeBytesLength;

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
            this.payloadLenght = message.payloadLenght;
            try
            {
                Array.Copy(message.payload, this.payload, message.payload.Length);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while copying payload in Copy Constructor",
                    ex);
            }
        }

        public InverterMessage(byte[] rawMessage)
        {
            this.responseMessage = true;

            this.heartbeatMessage = false;

            if (rawMessage[1] == 0x00)
            {
                throw new InverterDriverException($"Invalid raw data");
            }

            this.IsWriteMessage = (rawMessage[0] & 0x80) > 0;

            this.IsError = (rawMessage[0] & 0x40) > 0;

            this.payloadLenght = rawMessage[1] - 4;

            this.SystemIndex = rawMessage[2];

            this.DataSetIndex = rawMessage[3];

            this.parameterId =
                BitConverter.ToInt16(rawMessage,
                    4); //VALUE parameterId is always stored starting at byte intex 4 in the byte array

            this.payload = new byte[payloadLenght];
            try
            {
                Array.Copy(rawMessage, 6, this.payload, 0, payloadLenght);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while converting ParameterId to bytes", ex);
            }
        }

        public InverterMessage(byte systemIndex, short parameterId)
        {
            this.responseMessage = false;
            this.SystemIndex = systemIndex;
            this.DataSetIndex = ActualDataSetIndex;
            this.parameterId = parameterId;
            this.IsWriteMessage = false;
            this.heartbeatMessage = false;
        }

        public InverterMessage(byte systemIndex, short parametrerId, object payload)
        {
            this.responseMessage = false;
            this.SystemIndex = systemIndex;
            this.DataSetIndex = ActualDataSetIndex;
            this.parameterId = parametrerId;
            this.IsWriteMessage = true;
            this.heartbeatMessage = false;

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
            }

            this.payloadLenght = this.payload.Length;
        }

        #endregion

        #region Properties

        public byte BytePayload => this.ConvertPayloadToByte();

        public byte DataSetIndex { get; }

        public double DoublePayload => this.ConvertPayloadToDouble();

        public float FloatPayload => this.ConvertPayloadToFloat();

        public int IntPayload => this.ConvertPayloadToInt();

        public bool IsError { get; }

        public bool IsHeartbeatMessage => this.heartbeatMessage;

        public bool IsResponseMessage => this.responseMessage;

        public bool IsWriteMessage { get; }

        public InverterParameterId ParameterId => (InverterParameterId)this.parameterId;

        public object Payload => this.ConvertPayload();

        public short ShortPayload => this.ConvertPayloadToShort();

        public string StringPayload => this.ConvertPayloadToString();

        public byte SystemIndex { get; }

        public ushort UShortPayload => this.ConvertPayloadToUShort();

        #endregion

        #region Methods

        /// <summary>
        ///     Returns a byte array from the current Inverter Message ready to be sent to the Inverter hardware
        /// </summary>
        /// <returns>Byte array containing the request message for the inverter</returns>
        /// <exception cref="InverterDriverException">Configured ParameterId failed to convert into byte array</exception>
        public byte[] GetReadMessage()
        {
            var readMessage = new byte[6];

            readMessage[0] = ReadHeader;
            readMessage[1] = 0x04; //VALUE Fixed packed data length to 4 bytes
            readMessage[2] = this.SystemIndex;
            readMessage[3] = this.DataSetIndex;

            if (this.parameterId.Equals(InverterParameterId.ControlWordParam) || IsWriteMessage)
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

        public byte[] GetWriteMessage()
        {
            if (this.parameterId.Equals(InverterParameterId.StatusWordParam) || !IsWriteMessage)
            {
                throw new InverterDriverException("Invalid Operation", InverterDriverExceptionCode.RequerstWriteOnReadOnlyParameter);
            }

            var messageLength = this.payload.Length + 6;
            var writeMessage = new byte[messageLength];

            writeMessage[0] = WriteHeader;
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

        public byte[] GteHeartbeatMessage(bool setBit)
        {
            if (this.parameterId != (short)InverterParameterId.ControlWordParam)
                throw new InverterDriverException("Invalid parameter id");

            this.heartbeatMessage = true;

            //VALUE 14th byte of control word value represents Heartbeat flag
            if (setBit)
            {
                this.payload[0] |= 0x40;
            }
            else
            {
                this.payload[0] &= 0xBF;
            }

            return this.GetWriteMessage();
        }

        private object ConvertPayload()
        {
            var returnValue = default(object);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.SetOperatingModeParam:
                    if (this.payloadLenght == 2) returnValue = BitConverter.ToUInt16(this.payload);
                    break;

                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                    if (this.payloadLenght == 4) returnValue = BitConverter.ToInt32(this.payload);
                    break;
            }

            return returnValue;
        }

        private byte ConvertPayloadToByte()
        {
            return default(byte);
        }

        private double ConvertPayloadToDouble()
        {
            return default(double);
        }

        private float ConvertPayloadToFloat()
        {
            return default(float);
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
                case InverterParameterId.StatusDigitalSignals:
                    if (this.payloadLenght == 4) returnValue = BitConverter.ToInt32(this.payload);
                    break;

                default:
                    returnValue = default(int);
                    break;
            }

            return returnValue;
        }

        private short ConvertPayloadToShort()
        {
            return default(short);
        }

        private string ConvertPayloadToString()
        {
            return default(string);
        }

        private ushort ConvertPayloadToUShort()
        {
            var returnValue = default(ushort);

            switch ((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.SetOperatingModeParam:
                    if (this.payloadLenght == 2) returnValue = BitConverter.ToUInt16(this.payload);
                    break;

                default:
                    returnValue = default(ushort);
                    break;
            }

            return returnValue;
        }

        #endregion
    }
}
