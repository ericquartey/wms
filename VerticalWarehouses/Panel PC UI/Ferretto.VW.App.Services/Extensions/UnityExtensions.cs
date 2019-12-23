using System;
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
            _ = containerRegistry ?? throw new ArgumentNullException(nameof(containerRegistry));
            _ = serviceUrl ?? throw new ArgumentNullException(nameof(serviceUrl));

            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
            containerRegistry.RegisterSingleton<ISessionService, SessionService>();
            containerRegistry.RegisterSingleton<IHubNotificationService, HubNotificationService>();
            containerRegistry.RegisterSingleton<IMachineModeService, MachineModeService>();
            containerRegistry.RegisterSingleton<IMachineElevatorService, MachineElevatorService>();

            containerRegistry.RegisterSingleton<ISensorsService, SensorsService>();
            containerRegistry.RegisterSingleton<IMachineService, MachineService>();

            containerRegistry.RegisterSingleton<IMachineErrorsService, MachineErrorsService>();

            // Operator
            containerRegistry.RegisterSingleton<IWmsDataProvider, WmsDataProvider>();
            containerRegistry.RegisterSingleton<IWmsImagesProvider, WmsImagesProvider>();
            containerRegistry.RegisterSingleton<IMissionOperationsService, MissionOperationsService>();

            _ = containerRegistry.GetContainer().RegisterSingleton<IHealthProbeService>(
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
            _ = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));

            // force the instantiation of the services
            _ = containerProvider.Resolve<IHubNotificationService>();
            _ = containerProvider.Resolve<IMachineModeService>();
            _ = containerProvider.Resolve<IMachineService>();

            containerProvider.Resolve<IHealthProbeService>().Start();
            containerProvider.Resolve<IMachineService>().ServiceStart();

            return containerProvider;
        }

        #endregion
    }
}
