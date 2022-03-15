using System.Net;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IProxyProvider
    {
        #region Methods

        Proxy Get();

        WebProxy GetWebProxy();

        void SaveAsync(Proxy proxy);

        #endregion
    }
}
