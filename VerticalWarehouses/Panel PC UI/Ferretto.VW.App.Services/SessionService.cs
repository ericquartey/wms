using Ferretto.VW.App.Services.Interfaces;
using System;
#if DEBUG
using System.Windows;

#else
using System.Diagnostics;
#endif

namespace Ferretto.VW.App.Services
{
    internal class SessionService : ISessionService
    {
        private readonly IHealthProbeService healthProbeService;

        private readonly INavigationService navigationService;

        public SessionService(IHealthProbeService healthProbeService, INavigationService navigationService)
        {
            if (healthProbeService is null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            if (navigationService is null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.healthProbeService = healthProbeService;
            this.navigationService = navigationService;

            this.healthProbeService.HealthStatusChanged.Subscribe(
                this.OnHealthStatusChanged,
                Prism.Events.ThreadOption.UIThread,
                false);
        }

        private void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Unhealthy)
            {
                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Login),
                    Utils.Modules.Login.LOGIN);
            }
        }

        #region Methods

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

        #endregion
    }
}
