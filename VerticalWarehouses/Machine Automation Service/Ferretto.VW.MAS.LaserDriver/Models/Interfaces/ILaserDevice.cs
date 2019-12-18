using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal interface ILaserDevice
    {
        IPAddress IpAddress { get; set; }
        int TcpPort { get; set; }
    }
}
