namespace Ferretto.VW.App.Services
{
    public interface IHealthProbeService
    {
        #region Properties

        HealthStatus HealthStatus { get; }

        HealthStatusChangedPubSubEvent HealthStatusChanged { get; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        #endregion
    }
}
