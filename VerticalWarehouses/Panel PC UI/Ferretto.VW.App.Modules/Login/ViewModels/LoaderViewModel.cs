using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    [Warning(WarningsArea.Login)]
    internal sealed class LoaderViewModel : BaseMainViewModel
    {
        #region Fields

        private const int FirewallCheckDelay = 5000;

        private readonly IBayManager bayManager;

        private readonly IHealthProbeService healthProbeService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string applicationVersion;

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

            var versionAttribute = this.GetType().Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)
                .FirstOrDefault() as AssemblyInformationalVersionAttribute;

            var versionString = versionAttribute?.InformationalVersion ?? this.GetType().Assembly.GetName().Version.ToString();

            this.ApplicationVersion = string.Format(Resources.VWApp.Version, versionString);
        }

        #endregion

        #region Properties

        public string ApplicationVersion
        {
            get => this.applicationVersion;
            set => this.SetProperty(ref this.applicationVersion, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            SplashScreenService.Hide();

            await base.OnAppearedAsync();

            this.subscriptionToken = this.subscriptionToken
                ??
                this.healthProbeService.HealthStatusChanged
                    .Subscribe(
                        async (e) => await this.OnHealthStatusChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.OnHealthStatusChangedAsync(null);
        }

        private async Task CheckFirewallStatusAsync()
        {
            await Task.Delay(FirewallCheckDelay);

            if (this.healthProbeService.HealthStatus == HealthStatus.Unknown)
            {
                try
                {
                    var firewallManagerType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                    var firewallManager = (dynamic)Activator.CreateInstance(firewallManagerType);
                    bool isFirewallenabled = firewallManager.LocalPolicy.CurrentProfile.FirewallEnabled;

                    if (isFirewallenabled)
                    {
                        this.ShowNotification(
                            Resources.InstallationApp.FirewallIsEnabledOnThisTerminal,
                            Services.Models.NotificationSeverity.Warning);
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private void NavigateToLoginPage(MAS.AutomationService.Contracts.MachineIdentity machineIdentity)
        {
            this.ClearNotifications();

            this.NavigationService.Appear(
                nameof(Utils.Modules.Login),
                Utils.Modules.Login.LOGIN,
                machineIdentity,
                trackCurrentView: false);
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await this.RetrieveMachineInfoAsync();
        }

        private async Task RetrieveMachineInfoAsync()
        {
            this.logger.Info($"Status of machine automation service is '{this.healthProbeService.HealthStatus}'.");

            switch (this.healthProbeService.HealthStatus)
            {
                case HealthStatus.Initialized:
                case HealthStatus.Initializing:
                    this.ShowNotification("I servizi sono in fase di inizializzazione.");

                    break;

                case HealthStatus.Healthy:
                case HealthStatus.Degraded:

                    this.ShowNotification("Connessione ai servizi stabilita.", Services.Models.NotificationSeverity.Success);

                    try
                    {
                        await this.bayManager.InitializeAsync();
                        var machineIdentity = this.bayManager.Identity;

                        this.NavigateToLoginPage(machineIdentity);
                    }
                    catch (Exception ex)
                    {
                        this.ShowNotification(ex);
                    }

                    break;

                case HealthStatus.Unhealthy:

                    this.ShowNotification("Impossibile connettersi al servizio di automazione", Services.Models.NotificationSeverity.Error);
                    break;
            }
        }

        #endregion
    }
}
