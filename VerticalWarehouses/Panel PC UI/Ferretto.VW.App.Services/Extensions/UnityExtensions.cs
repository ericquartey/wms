using System;
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

        public static IContainerRegistry RegisterAppServices(
            this IContainerRegistry containerRegistry,
            Uri serviceMasUrl,
            string serviceLiveHealthPath,
            string serviceReadyHealthPath,
            string masServiceName,
            bool isMaster)
        {
            _ = containerRegistry ?? throw new ArgumentNullException(nameof(containerRegistry));
            _ = serviceMasUrl ?? throw new ArgumentNullException(nameof(serviceMasUrl));

            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
            containerRegistry.RegisterSingleton<ISessionService, SessionService>();
            containerRegistry.RegisterSingleton<IHubNotificationService, HubNotificationService>();
            containerRegistry.RegisterSingleton<IMachineModeService, MachineModeService>();
            containerRegistry.RegisterSingleton<IMachineElevatorService, MachineElevatorService>();
            containerRegistry.RegisterSingleton<ITimeSyncService, TimeSyncService>();
            containerRegistry.RegisterSingleton<IUsbWatcherService, UsbWatcherService>();

            containerRegistry.RegisterSingleton<ISerialPortsService, SerialPortsService>();
            containerRegistry.RegisterSingleton<ILocalizationService, LocalizationService>();

            containerRegistry.RegisterSingleton<ISensorsService, SensorsService>();
            containerRegistry.RegisterSingleton<IMachineService, MachineService>();

            containerRegistry.RegisterSingleton<IMachineErrorsService, MachineErrorsService>();
            containerRegistry.RegisterSingleton<ILaserPointerService, LaserPointerService>();
            containerRegistry.RegisterSingleton<IAlphaNumericBarService, AlphaNumericBarService>();

            _ = containerRegistry.GetContainer().RegisterSingleton<IHealthProbeService>(
                 new InjectionFactory(c =>
                     new HealthProbeService(
                         serviceMasUrl,
                         serviceLiveHealthPath,
                         serviceReadyHealthPath,
                         masServiceName,
                         isMaster,
                         c.Resolve<IMachineWmsStatusWebService>(),
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
            containerProvider.Resolve<IMachineService>().StartAsync();
            containerProvider.Resolve<ITimeSyncService>().Start();
            containerProvider.Resolve<ISerialPortsService>().Start();
            containerProvider.Resolve<ILaserPointerService>().StartAsync();
            containerProvider.Resolve<IAlphaNumericBarService>().StartAsync();

            return containerProvider;
        }

        #endregion
    }
}
