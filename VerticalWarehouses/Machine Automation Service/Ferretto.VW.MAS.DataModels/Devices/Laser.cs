using System;
using System.Net;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Laser : DataModel
    {
        #region Fields

        private int tcpPort;

        #endregion

        #region Properties

        [JsonIgnore]
        public Bay Bay { get; set; }

        public int BayId { get; set; }

        public IPAddress IpAddress { get; set; }

        public int TcpPort
        {
            get => this.tcpPort;
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw new ArgumentException("The TCP port is not in the allowed range of values.", nameof(value));
                }

                this.tcpPort = value;
            }
        }

        #endregion
    }
}
