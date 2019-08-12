using System;

namespace Ferretto.VW.App.Services
{
    public interface IHealthProbeService
    {
        #region Properties

        HealthStatus HealthStatus { get; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        object SubscribeOnHealthStatusChanged(Action<HealthStatusChangedEventArgs> action);

        void UnSubscribe(object subscriptionToken);

        #endregion
    }
}
