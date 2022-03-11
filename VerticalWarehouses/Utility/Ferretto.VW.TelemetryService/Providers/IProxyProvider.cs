using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IProxyProvider
    {
        #region Methods

        IProxy? Get();

        void SaveAsync(IProxy proxy);

        #endregion
    }
}
