using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.SocketLink.Models;

namespace Ferretto.VW.MAS.SocketLink
{
    internal interface ISocketLinkSyncProvider
    {
        #region Methods

        string PeriodicResponse(List<SocketLinkCommand.HeaderType> typeOfResponses);

        string ProcessCommands(string buffer);

        #endregion
    }
}
