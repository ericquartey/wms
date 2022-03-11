using System.Net;

namespace Ferretto.VW.Common.Hubs
{
    public sealed class ProxyChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public ProxyChangedEventArgs(WebProxy proxy)
        {
            this.Proxy = proxy;
        }

        #endregion

        #region Properties

        public WebProxy Proxy { get; }

        #endregion
    }
}
