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
using Microsoft.AppCenter.Analytics;
using System.Collections.Generic;

namespace Ferretto.VW.App.Services
{
    internal sealed class SessionService : ISessionService
    {
        private readonly IAuthenticationService authenticationService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IHealthProbeService healthProbeService;

        private readonly INavigationService navigationService;

        private readonly IEventAggregator eventAggregator;

        private readonly IBayManager bayManager;

        public UserAccessLevel UserAccessLevel { get; private set; }

        public MachineIdentity MachineIdentity { get; set; }

        public SessionService(
            IAuthenticationService authenticationService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IHealthProbeService healthProbeService,
            INavigationService navigationService)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.bayManager = bayManager;
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            this.healthProbeService.HealthStatusChanged.Subscribe(
                async m => await this.OnHealthStatusChanged(m),
                ThreadOption.UIThread,
                false);

            this.eventAggregator
                .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                .Subscribe(
                    this.OnNavigation,
                    ThreadOption.BackgroundThread,
                    false);
        }

        private void OnNavigation(NavigationCompletedEventArgs e)
        {
            Analytics.TrackEvent("Page Visit", new Dictionary<string, string> {
                    { "Name", e.ViewModelName?.Replace("ViewModel", "View") },
                    { "Machine Serial Number", this.bayManager.Identity?.SerialNumber }
                });
        }

        private async Task OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e.HealthMasStatus == HealthStatus.Unhealthy
                &&
                ConfigurationManager.AppSettings.LogoutWhenUnhealthy())
            {
                this.logger.Debug($"OnHealthStatusChanged.LogOutAsync();");

                await this.authenticationService.LogOutAsync();

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Login),
                    Utils.Modules.Login.LOGIN);
            }
        }

        public bool Shutdown()
        {
            try
            {
                var fullscreen = Convert.ToBoolean(ConfigurationManager.AppSettings["FullScreen"]);
                //#if DEBUG
                if (!fullscreen)
                {
                    System.Windows.Application.Current.Shutdown();
                }
                //#else
                else
                {
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo("shutdown", "/s /t 5");
                    processStartInfo.CreateNoWindow = true;
                    processStartInfo.UseShellExecute = false;
                    System.Diagnostics.Process.Start(processStartInfo);
                }
                //#endif
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
