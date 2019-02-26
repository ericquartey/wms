using System;
using System.Text;
using Ferretto.VW.Common_Utils.Exceptions;

namespace Ferretto.VW.InverterDriver
{
    public class InverterMessage
    {
        #region Fields

        private const Byte ActualDataSetIndex = 0x05; //VALUE fixed data set id used for the inverter operation

        private const Byte ReadHeader = 0x00;

        private const Byte WriteHeader = 0x80;

        private readonly Int16 parameterId;

        private readonly Byte[] payload;

        private Byte writeBytesLength;

        #endregion

        #region Constructors

        public InverterMessage(InverterMessage message)
        {
            this.IsWriteMessage = message.IsWriteMessage;
            this.SystemIndex = message.SystemIndex;
            this.DataSetIndex = message.DataSetIndex;
            this.IsError = message.IsError;
            this.parameterId = message.parameterId;
            this.payload = new Byte[message.payload.Length];
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

        public InverterMessage(Byte[] rawMessage)
        {
            this.IsWriteMessage = ( rawMessage[0] & 0x80 ) > 0;

            this.IsError = ( rawMessage[0] & 0x40 ) > 0;

            this.SystemIndex = rawMessage[2];

            this.DataSetIndex = rawMessage[3];

            this.parameterId =
                BitConverter.ToInt16(rawMessage,
                    4); //VALUE parameterId is always stored starting at byte intex 4 in the byte array

            var payloadSize =
                rawMessage.Length -
                6; //VALUE In received messages payload is always 6 bytes shorter than the full message

            this.payload = new Byte[payloadSize];
            try
            {
                Array.Copy(rawMessage, 6, this.payload, 0, payloadSize);
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while converting ParameterId to bytes", ex);
            }
        }

        public InverterMessage(Byte systemIndex, Int16 parameterId)
        {
            this.SystemIndex = systemIndex;
            this.DataSetIndex = ActualDataSetIndex;
            this.parameterId = parameterId;
            this.IsWriteMessage = false;
        }

        public InverterMessage(Byte systemIndex, Int16 parametrerId, Object payload)
        {
            this.SystemIndex = systemIndex;
            this.DataSetIndex = ActualDataSetIndex;
            this.parameterId = parametrerId;
            this.IsWriteMessage = true;

            var payloadType = payload.GetType();

            switch (payloadType.Name)
            {
                case "Byte":
                    this.payload = new[] {(Byte) payload};
                    break;

                case "Int16":
                    this.payload = BitConverter.GetBytes((Int16) payload);
                    break;

                case "UInt16":
                    this.payload = BitConverter.GetBytes((UInt16) payload);
                    break;

                case "Int32":
                    this.payload = BitConverter.GetBytes((Int32) payload);
                    break;

                case "Single":
                    this.payload = BitConverter.GetBytes((Single) payload);
                    break;

                case "Double":
                    this.payload = BitConverter.GetBytes((Double) payload);
                    break;

                case "String":
                    this.payload = Encoding.ASCII.GetBytes((String) payload);
                    break;
            }
        }

        #endregion

        #region Properties

        public Byte BytePayload => this.ConvertPayloadToByte();

        public Byte DataSetIndex { get; }

        public Double DoublePayload => this.ConvertPayloadToDouble();

        public Single FloatPayload => this.ConvertPayloadToFloat();

        public Int32 IntPayload => this.ConvertPayloadToInt();

        public Boolean IsError { get; }

        public Boolean IsWriteMessage { get; }

        public InverterParameterId ParameterId => (InverterParameterId) this.parameterId;

        public Object Payload => this.ConvertPayload();

        public Int16 ShortPayload => this.ConvertPayloadToShort();

        public String StringPayload => this.ConvertPayloadToString();

        public Byte SystemIndex { get; }

        public UInt16 UShortPayload => this.ConvertPayloadToUShort();

        #endregion

        #region Methods

        /// <summary>
        ///     Returns a byte array from the current Inverter Message ready to be sent to the Inverter hardware
        /// </summary>
        /// <returns>Byte array containing the request message for the inverter</returns>
        /// <exception cref="InverterDriverException">Configured ParameterId failed to convert into byte array</exception>
        public Byte[] GetReadMessage()
        {
            var readMessage = new Byte[6];

            readMessage[0] = ReadHeader;
            readMessage[1] = 0x04; //VALUE Fixed packed data length to 4 bytes
            readMessage[2] = this.SystemIndex;
            readMessage[3] = this.DataSetIndex;

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

        public Byte[] GetWriteMessage()
        {
            var messageLength = this.payload.Length + 6;
            var writeMessage = new Byte[messageLength];

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

        public Byte[] GteHeartbeatMessage()
        {
            if (this.parameterId != (Int16) InverterParameterId.ControlWordParam)
                throw new InverterDriverException("Invalid parameter id");

            var heartbeatMessage = this.GetWriteMessage();

            //VALUE 14th byte of control word value represents Heartbeat flag
            heartbeatMessage[7] |= 0x40;

            return heartbeatMessage;
        }

        private Object ConvertPayload()
        {
            var returnValue = default(Object);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                    if (this.payload.Length == 2) returnValue = BitConverter.ToUInt16(this.payload);
                    break;

                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    if (this.payload.Length == 1) returnValue = BitConverter.ToBoolean(this.payload);
                    break;

                default:
                    returnValue = default(Object);
                    break;
            }

            return returnValue;
        }

        private Byte ConvertPayloadToByte()
        {
            var returnValue = default(Byte);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(Byte);
                    break;
            }

            return returnValue;
        }

        private Double ConvertPayloadToDouble()
        {
            var returnValue = default(Double);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(Double);
                    break;
            }

            return returnValue;
        }

        private Single ConvertPayloadToFloat()
        {
            var returnValue = default(Single);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(Single);
                    break;
            }

            return returnValue;
        }

        private Int32 ConvertPayloadToInt()
        {
            var returnValue = default(Int32);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(Int32);
                    break;
            }

            return returnValue;
        }

        private Int16 ConvertPayloadToShort()
        {
            var returnValue = default(Int16);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(Int16);
                    break;
            }

            return returnValue;
        }

        private String ConvertPayloadToString()
        {
            var returnValue = default(String);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(String);
                    break;
            }

            return returnValue;
        }

        private UInt16 ConvertPayloadToUShort()
        {
            var returnValue = default(UInt16);

            switch ((InverterParameterId) this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                case InverterParameterId.HomingCreepSpeedParam:
                case InverterParameterId.HomingFastSpeedParam:
                case InverterParameterId.HomingModeParam:
                case InverterParameterId.HomingOffsetParam:
                case InverterParameterId.HomingAcceleration:
                case InverterParameterId.PositionAccelerationParam:
                case InverterParameterId.PositionDecelerationParam:
                case InverterParameterId.PositionTargetPositionParam:
                case InverterParameterId.PositionTargetSpeedParam:
                case InverterParameterId.SetOperatingModeParam:
                case InverterParameterId.StatusWordParam:
                case InverterParameterId.ActualPositionShaft:
                case InverterParameterId.StatusDigitalSignals:
                case InverterParameterId.ControlModeParam:
                case InverterParameterId.AnalogIcParam:
                    break;

                default:
                    returnValue = default(UInt16);
                    break;
            }

            return returnValue;
        }

        #endregion
    }
}
