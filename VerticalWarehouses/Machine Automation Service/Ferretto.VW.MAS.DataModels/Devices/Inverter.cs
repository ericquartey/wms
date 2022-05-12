using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Inverter : DataModel
    {
        #region Fields

        public const int MasterIndex = 0;

        private int tcpPort;

        #endregion

        #region Properties

        public int CanOpenNode => (int)this.Index + 1;

        public InverterIndex Index { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        public bool IsCanOpen => this.TcpPort == 0;

        public IEnumerable<InverterParameter> Parameters { get; set; }

        public int TcpPort
        {
            get => this.tcpPort;
            set
            {
                if (value < 0 || value > System.Net.IPEndPoint.MaxPort)
                {
                    throw new System.ArgumentException("The TCP port is not in the allowed range of values.", nameof(value));
                }

                this.tcpPort = value;
            }
        }

        public InverterType Type { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Inverter (index: {this.Index}, type: {this.Type})";
        }

        #endregion
    }
}
