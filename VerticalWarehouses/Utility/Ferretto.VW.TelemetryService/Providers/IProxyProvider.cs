using System.Net;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IProxyProvider
    {
        #region Methods

        IProxy Get();

        WebProxy GetWebProxy();

        void SaveAsync(IProxy proxy);

        #endregion
    }
}
