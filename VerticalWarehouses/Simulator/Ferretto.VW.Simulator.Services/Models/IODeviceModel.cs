using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Services.Models
{
    public class IODeviceModel
    {
        public int Id { get; set; }

        public byte FirmwareVersion { get; set; } = 0x10;

        public bool[] IOs { get; set; } = new bool[16];

        public ushort IOValue
        {
            get
            {
                ushort result = 0;
                for (int i = 0; i < this.IOs.Length; i++)
                {
                    if (this.IOs[i])
                    {
                        result += (ushort)Math.Pow(2, i);
                    }
                }
                return result;
            }
        }

        public byte[] Buffer;

    }
}
