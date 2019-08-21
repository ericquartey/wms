using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    public class LoaderViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IHealthProbeService healthProbeService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoaderViewModel(
            IBayManager bayManager,
            IHealthProbeService healthProbeService)
            : base(PresentationMode.Login)
        {
            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (healthProbeService is null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            this.bayManager = bayManager;
            this.healthProbeService = healthProbeService;
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.subscriptionToken = this.healthProbeService.HealthStatusChanged.Subscribe(
                async (e) => await this.OnHealthStatusChanged(e),
                ThreadOption.UIThread,
                false);
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.subscriptionToken != null)
            {
                this.healthProbeService.HealthStatusChanged.Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        private void NavigateToLoginPage(MAS.AutomationService.Contracts.MachineIdentity machineIdentity)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Login),
                Utils.Modules.Login.LOGIN,
                machineIdentity,
                trackCurrentView: false);
        }

        private async Task OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            await this.RetrieveMachineInfoAsync();
        }

        private async Task RetrieveMachineInfoAsync()
        {
            this.logger.Info($"Status of machine automation service is '{this.healthProbeService.HealthStatus}'.");

            switch (this.healthProbeService.HealthStatus)
            {
                case HealthStatus.Healthy:
                case HealthStatus.Degraded:

                    await this.bayManager.InitializeAsync();
                    var machineIdentity = this.bayManager.Identity;

                    this.NavigateToLoginPage(machineIdentity);

                    break;

                case HealthStatus.Unhealthy:

                    this.ShowNotification("Impossibile connettersi al servizio di automazione", Services.Models.NotificationSeverity.Error);
                    break;
            }
        }

        #endregion
    }
}
