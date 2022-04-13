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

        public InverterIndex Index { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        public System.Net.IPAddress LocalIpAddress { get; set; }

        public IEnumerable<InverterParameter> Parameters { get; set; }

        public int TcpPort
        {
            get => this.tcpPort;
            set
            {
                if (value < System.Net.IPEndPoint.MinPort || value > System.Net.IPEndPoint.MaxPort)
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
