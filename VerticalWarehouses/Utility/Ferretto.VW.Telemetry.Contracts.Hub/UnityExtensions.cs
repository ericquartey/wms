using Prism.Ioc;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry RegisterTelemetryHub(
            this IContainerRegistry container,
            System.Uri telemetryHubPathUrl)
        {
            if (container is null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            if (telemetryHubPathUrl is null)
            {
                throw new System.ArgumentNullException(nameof(telemetryHubPathUrl));
            }

            container.RegisterInstance<ITelemetryHubClient>(new TelemetryHubClient(telemetryHubPathUrl));

            return container;
        }

        public static IContainerProvider UseTelemetryHubs(this IContainerProvider containerProvider)
        {
            if (containerProvider is null)
            {
                throw new System.ArgumentNullException(nameof(containerProvider));
            }

            containerProvider
               .Resolve<ITelemetryHubClient>()
               .ConnectAsync();

            return containerProvider;
        }

        #endregion
    }
}
