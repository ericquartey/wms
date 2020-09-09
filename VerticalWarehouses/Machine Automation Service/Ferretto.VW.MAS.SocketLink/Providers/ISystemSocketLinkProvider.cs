using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.SocketLink.Providers
{
    public interface ISystemSocketLinkProvider
    {
        #region Properties

        bool CanEnableSyncMode { get; set; }

        #endregion
    }
}
