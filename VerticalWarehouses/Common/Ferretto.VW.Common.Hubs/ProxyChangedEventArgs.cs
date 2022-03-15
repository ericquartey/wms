using System.Net;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.Common.Hubs
{
    public sealed class ProxyChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ProxyChangedEventArgs(Proxy proxy)
        {
            this.Proxy = proxy;
        }

        #endregion 

        #region Properties

        public Proxy Proxy { get; }

        #endregion 
    }
}
