using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal interface ILaserDevice
    {
        #region Properties

        IPAddress IpAddress { get; }

        int TcpPort { get; }

        #endregion

        #region Methods

        void DestroyStateMachine();

        #endregion
    }
}
