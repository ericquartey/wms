using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal sealed class LaserDevice : ILaserDevice
    {
        #region Constructors

        public LaserDevice(IPAddress ipAddress, int port)
        {
            this.IpAddress = ipAddress;
            this.TcpPort = port;
        }

        #endregion

        #region Properties

        public IPAddress IpAddress { get; set; }

        public int TcpPort { get; set; }

        #endregion
    }
}
