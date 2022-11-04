using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.SocketLink.Models;

namespace Ferretto.VW.MAS.SocketLink
{
    public interface ISocketLinkSyncProvider
    {
        #region Methods

        string GetVersion();

        string PeriodicResponse(List<SocketLinkCommand.HeaderType> typeOfResponses, bool isLineFeed);

        string ProcessCommands(string buffer, bool isLineFeed);

        #endregion
    }
}
