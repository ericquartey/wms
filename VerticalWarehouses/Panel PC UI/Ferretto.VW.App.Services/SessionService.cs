using System;
using System.Configuration;

#if DEBUG
using System.Windows;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

#else
using System.Diagnostics;
#endif

namespace Ferretto.VW.App.Services
{
    internal class SessionService : ISessionService
    {
        private readonly IHealthProbeService healthProbeService;

        private readonly INavigationService navigationService;

        private readonly IEventAggregator eventAggregator;

        public UserAccessLevel UserAccessLevel { get; private set; }

        public MachineIdentity MachineIdentity { get; set; }

        public SessionService(
            IEventAggregator eventAggregator,
            IHealthProbeService healthProbeService,
            INavigationService navigationService)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            this.healthProbeService.HealthStatusChanged.Subscribe(
                this.OnHealthStatusChanged,
                Prism.Events.ThreadOption.UIThread,
                false);
        }

        private void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Unhealthy
                &&
                ConfigurationManager.AppSettings.LogoutWhenUnhealthy())
            {
                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Login),
                    Utils.Modules.Login.LOGIN);
            }
        }

        public bool Shutdown()
        {
            try
            {
#if DEBUG
                Application.Current.Shutdown();
#else
                var processStartInfo = new ProcessStartInfo("shutdown", "/s /t 5");
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                Process.Start(processStartInfo);
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetUserAccessLevel(UserAccessLevel userAccessLevel)
        {
            this.UserAccessLevel = userAccessLevel;

            this.eventAggregator
              .GetEvent<UserAccessLevelNotificationPubSubEvent>()
              .Publish(new UserAccessLevelNotificationMessage(userAccessLevel));
        }
    }
}
