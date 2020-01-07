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
            string installationHubPath)
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

            container.RegisterInstance<IOperatorHubClient>(new OperatorHubClient(operatorHubUrl));
            container.RegisterInstance<IInstallationHubClient>(new InstallationHubClient(webServiceUrl, installationHubPath));

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

            return containerProvider;
        }

        #endregion
    }
}
