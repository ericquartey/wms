using System;
using System.Collections;
using System.Linq;

namespace Ferretto.VW.InverterDriver
{
    public class Telegram
    {
        #region Fields

        public const byte Read_Header = 0x00;

        public const byte Read_nBytes = 0x04;

        public const byte Write_Header = 0x80;

        private readonly int N_BITS_8 = 8;

        private int Write_nBytes_Total;

        #endregion

        #region Properties

        public ParameterID ParameterIDFromParse { get; private set; }

        public object RetValueFromParse { get; private set; }

        #endregion

        #region Methods

        public byte[] BuildReadPacket(byte Read_SystemIndex, byte Read_DataSetIndex, short Read_ParamID)
        {
            var ReadPacket = new byte[6];
            ReadPacket[0] = Read_Header;
            ReadPacket[1] = Read_nBytes;
            ReadPacket[2] = Convert.ToByte(Read_SystemIndex);
            ReadPacket[3] = Convert.ToByte(Read_DataSetIndex);
            ///<summary>
            ///Read_ParamID
            ///</summary>
            var ans_Read_ParamID = new byte[2];
            var parameterNo_Read_ParamID = new byte[sizeof(short)];
            parameterNo_Read_ParamID = BitConverter.GetBytes(Read_ParamID);
            parameterNo_Read_ParamID.CopyTo(ans_Read_ParamID, 0);
            Array.Copy(ans_Read_ParamID, 0, ReadPacket, 4, 2);
            return ReadPacket;
        }

        public byte[] BuildWritePacket(byte Write_SystemIndex, byte Write_DataSetIndex, short Write_ParamID, ValueDataType Write_DataType, object value)
        {
            var nBytesPayLoad = 0;
            switch (Write_DataType)
            {
                case ValueDataType.Byte: nBytesPayLoad = 1; break;
                case ValueDataType.Int16: nBytesPayLoad = 2; break;
                case ValueDataType.UInt16: nBytesPayLoad = 2; break;
                case ValueDataType.Int32: nBytesPayLoad = 4; break;
                case ValueDataType.Float: nBytesPayLoad = 4; break;
                case ValueDataType.Double: nBytesPayLoad = 8; break;
                case ValueDataType.String: nBytesPayLoad = Convert.ToString(value).Length; break;
                default: break;
            }
            this.Write_nBytes_Total = 1 + 1 + 1 + 1 + 2 + nBytesPayLoad;

            var WritePacket = new byte[this.Write_nBytes_Total];
            WritePacket[0] = Write_Header;
            WritePacket[1] = Convert.ToByte(this.Write_nBytes_Total - 2);
            WritePacket[2] = Convert.ToByte(Write_SystemIndex);
            WritePacket[3] = Convert.ToByte(Write_DataSetIndex);

            ///<summary>
            ///Write_ParamID
            ///</summary>
            var ans_Write_paramID = new byte[2];
            var parameterNo_Write_paramID = new byte[sizeof(short)];
            parameterNo_Write_paramID = BitConverter.GetBytes(Write_ParamID);
            parameterNo_Write_paramID.CopyTo(ans_Write_paramID, 0);
            Array.Copy(ans_Write_paramID, 0, WritePacket, 4, 2);

            ///<summary>
            ///Write_Data
            ///</summary>
            switch (Write_DataType)
            {
                case ValueDataType.Byte:

                    var ans_Write_data_byte = new byte[1];
                    var parameterNo_Write_data_byte = new byte[sizeof(byte)];
                    parameterNo_Write_data_byte = BitConverter.GetBytes(Convert.ToByte(value));
                    parameterNo_Write_data_byte.CopyTo(ans_Write_data_byte, 0);
                    Array.Copy(ans_Write_data_byte, 0, WritePacket, 6, 1);
                    break;

                case ValueDataType.Float:

                    var ans_Write_data_float = new byte[4];
                    var parameterNo_Write_data_float = new byte[sizeof(float)];
                    parameterNo_Write_data_float = BitConverter.GetBytes(Convert.ToSingle(value));
                    parameterNo_Write_data_float.CopyTo(ans_Write_data_float, 0);
                    Array.Copy(ans_Write_data_float, 0, WritePacket, 6, 4);
                    break;

                case ValueDataType.Double:

                    var ans_Write_data_double = new byte[8];
                    var parameterNo_Write_data_double = new byte[sizeof(double)];
                    parameterNo_Write_data_double = BitConverter.GetBytes(Convert.ToDouble(value));
                    parameterNo_Write_data_double.CopyTo(ans_Write_data_double, 0);
                    Array.Copy(ans_Write_data_double, 0, WritePacket, 6, 8);
                    break;

                case ValueDataType.Int16:

                    var ans_Write_data_Int16 = new byte[2];
                    var parameterNo_Write_data_Int16 = new byte[sizeof(short)];
                    parameterNo_Write_data_Int16 = BitConverter.GetBytes(Convert.ToInt16(value));
                    parameterNo_Write_data_Int16.CopyTo(ans_Write_data_Int16, 0);
                    Array.Copy(ans_Write_data_Int16, 0, WritePacket, 6, 2);
                    break;

                case ValueDataType.UInt16:

                    var ans_Write_data_UInt16 = new byte[2];
                    var parameterNo_Write_data_UInt16 = new byte[sizeof(ushort)];
                    parameterNo_Write_data_UInt16 = BitConverter.GetBytes(Convert.ToUInt16(value));
                    parameterNo_Write_data_UInt16.CopyTo(ans_Write_data_UInt16, 0);
                    Array.Copy(ans_Write_data_UInt16, 0, WritePacket, 6, 2);
                    break;

                case ValueDataType.Int32:

                    var ans_Write_data_Int32 = new byte[4];
                    var parameterNo_Write_data_Int32 = new byte[sizeof(int)];
                    parameterNo_Write_data_Int32 = BitConverter.GetBytes(Convert.ToInt32(value));
                    parameterNo_Write_data_Int32.CopyTo(ans_Write_data_Int32, 0);
                    Array.Copy(ans_Write_data_Int32, 0, WritePacket, 6, 4);
                    break;

                case ValueDataType.String:

                    var bytes = System.Text.Encoding.Unicode.GetBytes(Convert.ToString(value));
                    Array.Copy(bytes, 0, WritePacket, 6, bytes.Length);
                    break;

                default:
                    break;
            }
            return WritePacket;
        }

        public void ParseDataBuffer(byte[] telegram, int nBytes, out bool error)
        {
            error = false;
            var header = telegram[0];

            ///<summary>
            ///detect if there is an error on bit 6 of header
            ///</summary>
            var t = new BitArray(new byte[] { header });
            var bits = new bool[this.N_BITS_8];
            t.CopyTo(bits, 0);
            error = bits[6];

            var noBytes = telegram[1];
            var sys = telegram[2];
            var ds = telegram[3];

            ///<summary>
            ///ParseDataBuffer_ParamID
            ///</summary>
            var parameterNo = new byte[2];
            Array.Copy(telegram, 4, parameterNo, 0, 2);
            parameterNo.Reverse();
            this.ParameterIDFromParse = ParameterIDClass.Instance.ValueToParameterIDCode(BitConverter.ToInt16(parameterNo, 0));

            ///<summary>
            ///ParseDataBuffer_Data
            ///</summary>
            var nBytesPayLoad = nBytes - 6;
            if (nBytesPayLoad == 1)
            // type is byte
            {
                this.RetValueFromParse = telegram[6];
            }
            if (nBytesPayLoad == 2)
            // type is uint16
            {
                var value = new byte[2];
                Array.Copy(telegram, 6, value, 0, 2);
                value.Reverse();
                this.RetValueFromParse = BitConverter.ToUInt16(value, 0);
            }
            if (nBytesPayLoad == 4)
            // type is int32
            {
                var value = new byte[4];
                Array.Copy(telegram, 6, value, 0, 4);
                value.Reverse();
                this.RetValueFromParse = BitConverter.ToInt32(value, 0);
            }
            if (nBytesPayLoad > 4)
            // type is string
            {
                this.RetValueFromParse = BitConverter.ToString(telegram, 6);
            }

            return;
        }

        #endregion
    }
}
