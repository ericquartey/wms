using System.Net;

namespace Ferretto.ServiceDesk.Telemetry.Hubs
{
    public sealed class ProxyChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ProxyChangedEventArgs(IProxy proxy)
        {
            this.Proxy = proxy;
        }

        #endregion

        #region Properties

        public IProxy Proxy { get; }

        #endregion
    }
}
