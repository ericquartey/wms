using System;
using System.Text;
using Ferretto.VW.Common_Utils.Exceptions;

namespace Ferretto.VW.InverterDriver
{
    public class InverterMessage
    {
        #region Fields

        private const byte ReadHeader = 0x00;

        private const byte WriteHeader = 0x80;

        private byte dataSetIndex;

        private bool isError;

        private short parameterId;

        private byte[] payload;

        private byte systemIndex;

        private byte WriteBytesLength;

        #endregion

        #region Constructors

        public InverterMessage( byte[] rawMessage )
        {
            this.isError = (rawMessage[0] & 0x20) > 0;

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

        public InverterMessage( byte systemIndex, byte dataSetIndex, short parametrerId )
        {
            this.systemIndex = systemIndex;
            this.dataSetIndex = dataSetIndex;
            this.parameterId = parametrerId;
        }

        public InverterMessage( byte systemIndex, byte dataSetIndex, short parametrerId, object payload )
        {
            this.systemIndex = systemIndex;
            this.dataSetIndex = dataSetIndex;
            this.parameterId = parametrerId;

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

        public byte DataSetIndex => this.dataSetIndex;

        public bool IsError => this.isError;

        public InverterParameterId ParameterId => (InverterParameterId)this.parameterId;

        public object Payload => this.ConvertPayload();

        public byte SystemIndex => this.systemIndex;

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

        #endregion
    }
}
