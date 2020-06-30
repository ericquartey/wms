using Prism.Ioc;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry RegisterMasHubs(
            this IContainerRegistry container,
            System.Uri webServiceUrl,
            string operatorHubPath,
            string installationHubPath,
            string telemetryHubPath)
        {
            if (container is null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            if (webServiceUrl is null)
            {
                throw new System.ArgumentNullException(nameof(webServiceUrl));
            }

            var operatorHubUrl = new System.Uri(webServiceUrl, operatorHubPath);
            var telemetryHubUrl = new System.Uri(webServiceUrl, telemetryHubPath);

            container.RegisterInstance<IOperatorHubClient>(new OperatorHubClient(operatorHubUrl));
            container.RegisterInstance<IInstallationHubClient>(new InstallationHubClient(webServiceUrl, installationHubPath));
            container.RegisterInstance<ITelemetryHubClient>(new TelemetryHubClient(telemetryHubUrl));

            return container;
        }

        public static IContainerProvider UseMachineAutomationHubs(this IContainerProvider containerProvider)
        {
            if (containerProvider is null)
            {
                throw new System.ArgumentNullException(nameof(containerProvider));
            }

            containerProvider
                .Resolve<IInstallationHubClient>()
                .ConnectAsync();

            containerProvider
                .Resolve<IOperatorHubClient>()
                .ConnectAsync();

            //containerProvider
            //   .Resolve<ITelemetryHubClient>()
            //   .ConnectAsync(true);

            return containerProvider;
        }

        #endregion
    }
}
