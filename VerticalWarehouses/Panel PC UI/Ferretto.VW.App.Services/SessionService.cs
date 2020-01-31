#if DEBUG
using System.Windows;
#else
using System.Diagnostics;
#endif

using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class SessionService : ISessionService
    {
        private readonly IAuthenticationService autenticationService;

        private readonly IHealthProbeService healthProbeService;

        private readonly INavigationService navigationService;

        private readonly IEventAggregator eventAggregator;

        public UserAccessLevel UserAccessLevel { get; private set; }

        public MachineIdentity MachineIdentity { get; set; }

        public SessionService(
            IAuthenticationService autenticationService,
            IEventAggregator eventAggregator,
            IHealthProbeService healthProbeService,
            INavigationService navigationService)
        {
            this.autenticationService = autenticationService ?? throw new ArgumentNullException(nameof(autenticationService));
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            this.healthProbeService.HealthStatusChanged.Subscribe(
                async m => await this.OnHealthStatusChanged(m),
                Prism.Events.ThreadOption.UIThread,
                false);
        }

        private async Task OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e.HealthMasStatus == HealthStatus.Unhealthy
                &&
                ConfigurationManager.AppSettings.LogoutWhenUnhealthy())
            {
                await this.autenticationService.LogOutAsync();
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
