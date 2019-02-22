using System;
using System.Text;
using Ferretto.VW.Common_Utils.Exceptions;

namespace Ferretto.VW.InverterDriver
{
    public class InverterMessage
    {
        #region Fields

        private const byte ActualDataSetIndex = 0x05;    //VALUE fixed data set id used for the inverter operation

        private const byte ReadHeader = 0x00;

        private const byte WriteHeader = 0x80;

        private byte dataSetIndex;

        private bool isError;

        private bool isWriteMessage;

        private short parameterId;

        private byte[] payload;

        private byte systemIndex;

        private byte writeBytesLength;

        #endregion

        #region Constructors

        public InverterMessage( InverterMessage message )
        {
            this.isWriteMessage = message.isWriteMessage;
            this.systemIndex = message.systemIndex;
            this.dataSetIndex = message.dataSetIndex;
            this.isError = message.isError;
            this.parameterId = message.parameterId;
            this.payload = new byte[message.payload.Length];
            try
            {
                Array.Copy( message.payload, this.payload, message.payload.Length );
            }
            catch(Exception ex)
            {
                throw new InverterDriverException( $"Exception {ex.Message} while copying payload in Copy Constructor", ex );
            }
        }

        public InverterMessage( byte[] rawMessage )
        {
            this.isWriteMessage = (rawMessage[0] & 0x80) > 0;

            this.isError = (rawMessage[0] & 0x40) > 0;

            this.systemIndex = rawMessage[2];

            this.dataSetIndex = rawMessage[3];

            this.parameterId = BitConverter.ToInt16( rawMessage, 4 );   //VALUE parameterId is always stored starting at byte intex 4 in the byte array

            var payloadSize = rawMessage.Length - 6;    //VALUE In received messages payload is always 6 bytes shorter than the full message

            this.payload = new byte[payloadSize];
            try
            {
                Array.Copy( rawMessage, 6, this.payload, 0, payloadSize );
            }
            catch(Exception ex)
            {
                throw new InverterDriverException( $"Exception {ex.Message} while converting ParameterId to bytes", ex );
            }
        }

        public InverterMessage( byte systemIndex, short parameterId )
        {
            this.systemIndex = systemIndex;
            this.dataSetIndex = ActualDataSetIndex;
            this.parameterId = parameterId;
            this.isWriteMessage = false;
        }

        public InverterMessage( byte systemIndex, short parametrerId, object payload )
        {
            this.systemIndex = systemIndex;
            this.dataSetIndex = ActualDataSetIndex;
            this.parameterId = parametrerId;
            this.isWriteMessage = true;

            var payloadType = payload.GetType();

            switch(payloadType.Name)
            {
                case "Byte":
                    this.payload = new byte[] { (byte)payload };
                    break;

                case "Int16":
                    this.payload = BitConverter.GetBytes( (short)payload );
                    break;

                case "UInt16":
                    this.payload = BitConverter.GetBytes( (ushort)payload );
                    break;

                case "Int32":
                    this.payload = BitConverter.GetBytes( (int)payload );
                    break;

                case "Single":
                    this.payload = BitConverter.GetBytes( (float)payload );
                    break;

                case "Double":
                    this.payload = BitConverter.GetBytes( (double)payload );
                    break;

                case "String":
                    this.payload = Encoding.ASCII.GetBytes( (string)payload );
                    break;
            }
        }

        #endregion

        #region Properties

        public byte BytePayload => this.ConvertPayloadToByte();

        public byte DataSetIndex => this.dataSetIndex;

        public double DoublePayload => this.ConvertPayloadToDouble();

        public float FloatPayload => this.ConvertPayloadToFloat();

        public int IntPayload => this.ConvertPayloadToInt();

        public bool IsError => this.isError;

        public bool IsWriteMessage => this.isWriteMessage;

        public InverterParameterId ParameterId => (InverterParameterId)this.parameterId;

        public object Payload => this.ConvertPayload();

        public short ShortPayload => this.ConvertPayloadToShort();

        public string StringPayload => this.ConvertPayloadToString();

        public byte SystemIndex => this.systemIndex;

        public ushort UShortPayload => this.ConvertPayloadToUShort();

        #endregion

        #region Methods

        /// <summary>
        /// Returns a byte array from the current Inverter Message ready to be sent to the Inverter hardware
        /// </summary>
        /// <returns>Byte array containing the request message for the inverter</returns>
        /// <exception cref="InverterDriverException">Configured ParameterId failed to convert into byte array</exception>
        public byte[] GetReadMessage()
        {
            byte[] readMessage = new Byte[6];

            readMessage[0] = ReadHeader;
            readMessage[1] = 0x04; //VALUE Fixed packed data length to 4 bytes
            readMessage[2] = this.systemIndex;
            readMessage[3] = this.dataSetIndex;

            var parameterIdBytes = BitConverter.GetBytes( this.parameterId );

            try
            {
                Array.Copy( parameterIdBytes, 0, readMessage, 4, parameterIdBytes.Length );
            }
            catch(Exception ex)
            {
                throw new InverterDriverException( $"Exception {ex.Message} while converting ParameterId to bytes", ex );
            }

            return readMessage;
        }

        public byte[] GetWriteMessage()
        {
            int messageLength = this.payload.Length + 6;
            byte[] writeMessage = new Byte[messageLength];

            writeMessage[0] = WriteHeader;
            writeMessage[1] = Convert.ToByte( messageLength - 2 ); //VALUE Data length does not include header and length bytes
            writeMessage[2] = this.systemIndex;
            writeMessage[3] = this.dataSetIndex;

            var parameterIdBytes = BitConverter.GetBytes( this.parameterId );

            try
            {
                Array.Copy( parameterIdBytes, 0, writeMessage, 4, parameterIdBytes.Length );
            }
            catch(Exception ex)
            {
                throw new InverterDriverException( $"Exception {ex.Message} while converting ParameterId to bytes", ex );
            }

            try
            {
                Array.Copy( this.payload, 0, writeMessage, 6, this.payload.Length );
            }
            catch(Exception ex)
            {
                throw new InverterDriverException( $"Exception {ex.Message} while copying payload bytes to final message", ex );
            }

            return writeMessage;
        }

        public byte[] GteHeartbeatMessage()
        {
            if(this.parameterId != (short)InverterParameterId.ControlWordParam)
            {
                throw new InverterDriverException( $"Invalid parameter id" );
            }

            byte[] heartbeatMessage = this.GetWriteMessage();

            //VALUE 14th byte of control word value represents Heartbeat flag
            heartbeatMessage[7] |= 0x40;

            return heartbeatMessage;
        }

        private object ConvertPayload()
        {
            object returnValue = default( object );

            switch((InverterParameterId)this.parameterId)
            {
                case InverterParameterId.ControlWordParam:
                    if(this.payload.Length == 2)
                    {
                        returnValue = BitConverter.ToUInt16( this.payload );
                    }
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
                    if(this.payload.Length == 1)
                    {
                        returnValue = BitConverter.ToBoolean( this.payload );
                    }
                    break;

                default:
                    returnValue = default( object );
                    break;
            }

            return returnValue;
        }

        private byte ConvertPayloadToByte()
        {
            byte returnValue = default( byte );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( byte );
                    break;
            }

            return returnValue;
        }

        private double ConvertPayloadToDouble()
        {
            double returnValue = default( double );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( double );
                    break;
            }

            return returnValue;
        }

        private float ConvertPayloadToFloat()
        {
            float returnValue = default( float );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( float );
                    break;
            }

            return returnValue;
        }

        private int ConvertPayloadToInt()
        {
            int returnValue = default( int );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( int );
                    break;
            }

            return returnValue;
        }

        private short ConvertPayloadToShort()
        {
            short returnValue = default( short );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( short );
                    break;
            }

            return returnValue;
        }

        private string ConvertPayloadToString()
        {
            string returnValue = default( string );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( string );
                    break;
            }

            return returnValue;
        }

        private ushort ConvertPayloadToUShort()
        {
            ushort returnValue = default( ushort );

            switch((InverterParameterId)this.parameterId)
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
                    returnValue = default( ushort );
                    break;
            }

            return returnValue;
        }

        #endregion
    }
}
