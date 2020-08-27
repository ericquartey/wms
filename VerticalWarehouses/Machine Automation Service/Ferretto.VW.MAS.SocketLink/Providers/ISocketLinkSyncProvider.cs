using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.SocketLink
{
    internal interface ISocketLinkSyncProvider
    {
        #region Methods

        string ProcessCommands(string buffer);

        #endregion
    }
}
