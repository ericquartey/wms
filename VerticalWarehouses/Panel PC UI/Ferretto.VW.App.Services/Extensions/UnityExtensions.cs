using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.App.Services
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry RegisterUiServices(
            this IContainerRegistry containerRegistry,
            System.Uri serviceUrl,
            string serviceLiveHealthPath,
            string serviceReadyHealthPath)
        {
            if (containerRegistry is null)
            {
                throw new System.ArgumentNullException(nameof(containerRegistry));
            }

            if (serviceUrl is null)
            {
                throw new System.ArgumentNullException(nameof(serviceUrl));
            }

            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
            containerRegistry.RegisterSingleton<ISessionService, SessionService>();
            containerRegistry.RegisterSingleton<IHubNotificationService, HubNotificationService>();
            containerRegistry.RegisterSingleton<IMachineModeService, MachineModeService>();
            containerRegistry.RegisterSingleton<IMachineElevatorService, MachineElevatorService>();

            containerRegistry.RegisterSingleton<IMachineErrorsService, MachineErrorsService>();
            // Operator
            containerRegistry.RegisterSingleton<IWmsDataProvider, WmsDataProvider>();
            containerRegistry.RegisterSingleton<IWmsImagesProvider, WmsImagesProvider>();
            containerRegistry.RegisterSingleton<IMissionOperationsService, MissionOperationsService>();

            containerRegistry.GetContainer().RegisterSingleton<IHealthProbeService>(
                new InjectionFactory(c =>
                    new HealthProbeService(
                        serviceUrl,
                        serviceLiveHealthPath,
                        serviceReadyHealthPath,
                        c.Resolve<IEventAggregator>())));

            return containerRegistry;
        }

        public static IContainerProvider UseUiServices(this IContainerProvider containerProvider)
        {
            if (containerProvider is null)
            {
                throw new System.ArgumentNullException(nameof(containerProvider));
            }

            // force the instantiation of the services
            _ = containerProvider.Resolve<IHubNotificationService>();
            _ = containerProvider.Resolve<IMachineModeService>();

            containerProvider.Resolve<IHealthProbeService>().Start();

            return containerProvider;
        }

        #endregion
    }
}
