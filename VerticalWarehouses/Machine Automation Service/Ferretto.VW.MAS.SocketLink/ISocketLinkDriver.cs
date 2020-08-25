using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink
{
    internal interface ISocketLinkDriver
    {
        #region Methods

        SocketLinkCommand[] ParseReceivedCommands(string inputBuffer);

        #endregion
    }
}
