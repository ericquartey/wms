using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InverterDriver
{
    public class Telegram
    {
        #region -ReadPacket Properties-

        public const byte Read_Header = 0x00;
        public const byte Read_nBytes = 0x04;
        public Int16 Read_Sys { get; private set; }
        public Int16 Read_DataSet { get; private set; }
        public int Read_ParameterId { get; private set; }
       
        #endregion

        public void ReadPacket()
        {
            byte[] ReadPacket = new byte[5];
            ReadPacket[0] = Read_Header; 
            ReadPacket[1] = Read_nBytes;
            ReadPacket[2] = Convert.ToByte(this.Read_Sys);
            ReadPacket[3] = Convert.ToByte(this.Read_DataSet);
            ReadPacket[4] = Convert.ToByte(this.Read_ParameterId);
        }
        
        #region -WritePacket Properties-

        public const byte Write_Header = 0x80;
        public int Write_nBytes { get; private set; }
        public Int16 Write_Sys { get; private set; }
        public Int16 Write_DataSet { get; private set; }
        public int Write_ParameterId { get; private set; }
        public Int16 Write_Data { get; private set; } 
        #endregion

        public void WritePacket()
        {
            byte[] WritePacket = new byte[6];
            WritePacket[0] = Write_Header;
            WritePacket[1] = Convert.ToByte(this.Write_nBytes);
            WritePacket[2] = Convert.ToByte(this.Write_Sys);
            WritePacket[3] = Convert.ToByte(this.Write_DataSet);
            WritePacket[4] = Convert.ToByte(this.Write_ParameterId);
            WritePacket[5] = Convert.ToByte(this.Write_Data);

        }

    }
}
