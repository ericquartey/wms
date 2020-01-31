using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public interface IHealthProbeService
    {
        #region Properties

        HealthStatus HealthMasStatus { get; }

        PubSubEvent<HealthStatusChangedEventArgs> HealthStatusChanged { get; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        #endregion
    }
}
