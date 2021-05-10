using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public interface IHealthProbeService
    {
        #region Properties

        HealthStatus HealthMasStatus { get; }

        PubSubEvent<HealthStatusChangedEventArgs> HealthStatusChanged { get; }

        HealthStatus HealthWmsStatus { get; }

        #endregion

        #region Methods

        string ReloadMAS(int timeoutMilliseconds);

        void Start();

        void Stop();

        #endregion
    }
}
