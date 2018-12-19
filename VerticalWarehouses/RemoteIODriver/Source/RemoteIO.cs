using System.Collections.Generic;
using System.Net.Sockets;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System;

namespace Ferretto.VW.RemoteIODriver.Source
{
    public enum SensorsNames
    {
        LU_PRESENT_IN_BAY,
        SECURITY_FUNCTION_ACTIVE
    }

    public class RemoteIO
    {
        #region Fields

        private const ushort INPUT_ORIGIN = 0;
        private const ushort INPUT_QUANTITY = 8;
        private const ushort OUTPUT_ORIGIN = 3;
        private const ushort OUTPUT_QUANTITY = 5;
        private TcpClient client;
        private ModbusIpMaster master;

        #endregion Fields

        #region Constructors

        public RemoteIO()
        {
            this.Inputs = new List<bool>();
            this.Outputs = new List<bool>();
            this.Connect();
        }

        #endregion Constructors

        #region Destructors

        ~RemoteIO()
        {
            this.Disconnect();
        }

        #endregion Destructors

        #region Properties

        public List<bool> Inputs { get; set; }
        public string IPAddress { get; set; } = "169.254.231.10";
        public List<bool> Outputs { get; set; }
        public int Port { get; set; } = 502;

        #endregion Properties

        #region Methods

        public List<bool> ReadData()
        {
            this.Inputs.Clear();
            var tmp = this.master.ReadInputs(INPUT_ORIGIN, INPUT_QUANTITY);
            for (int i = 0; i < tmp.Length; i++)
            {
                this.Inputs.Add(tmp[i]);
            }
            //Console.WriteLine("RemoteIOReadData: " + this.Inputs[0] + ", " + this.Inputs[1] + ", " + this.Inputs[2] + ", " + this.Inputs[3] + ", " + this.Inputs[4] + ", " + this.Inputs[5] + ", " + this.Inputs[6] + ", " + this.Inputs[7] + ", ");
            return this.Inputs;
        }

        public void WriteData()
        {
            var tmp = new bool[] { false, false, false, false, false };
            if (tmp.Length != 0)
            {
                this.master.WriteMultipleCoils(OUTPUT_ORIGIN, tmp);
            }
        }

        private void Connect()
        {
            this.client = new TcpClient(this.IPAddress, this.Port);
            this.master = ModbusIpMaster.CreateIp(this.client);
        }

        private void Disconnect()
        {
            this.client.Close();
            this.client.Dispose();
        }

        #endregion Methods
    }
}
