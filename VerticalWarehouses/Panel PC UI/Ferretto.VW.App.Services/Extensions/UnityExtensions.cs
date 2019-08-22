using Ferretto.VW.App.Services.Interfaces;
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
            containerRegistry.RegisterSingleton<IStatusMessageService, StatusMessageService>();
            containerRegistry.RegisterSingleton<IBayManager, BayManager>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
            containerRegistry.RegisterSingleton<ISessionService, SessionService>();
            containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

            containerRegistry.GetContainer().RegisterSingleton<IHealthProbeService>(
                new InjectionFactory(c =>
                    new HealthProbeService(
                        serviceUrl,
                        serviceLiveHealthPath,
                        serviceReadyHealthPath,
                        c.Resolve<IEventAggregator>())));

            return containerRegistry;
        }

        #endregion
    }
}
