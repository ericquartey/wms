using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
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

        private readonly IMachineModeService machineModeService;

        private string applicationVersion;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoaderViewModel(
            IBayManager bayManager,
            IHealthProbeService healthProbeService,
            IMachineModeService machineModeService
            )
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
            this.ServiceHealthStatus = this.healthProbeService.HealthMasStatus;
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));

            var versionAttribute = Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)
                .FirstOrDefault() as AssemblyInformationalVersionAttribute;

            var versionString = versionAttribute?.InformationalVersion ?? this.GetType().Assembly.GetName().Version.ToString();

            this.ApplicationVersion = string.Format(Resources.Localized.Get("General.Version"), versionString);
        }

        #endregion

        #region Properties

        public string ApplicationVersion
        {
            get => this.applicationVersion;
            set => this.SetProperty(ref this.applicationVersion, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public override bool KeepAlive => false;

        public HealthStatus ServiceHealthStatus { get; set; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            SplashScreenService.Hide();

            this.subscriptionToken = this.subscriptionToken
                ??
                this.healthProbeService.HealthStatusChanged
                    .Subscribe(
                        async (e) => await this.OnHealthStatusChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.OnHealthStatusChangedAsync(null);

            await base.OnAppearedAsync();
        }

        protected override async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            // await base.OnHealthStatusChangedAsync(e);

            await this.RetrieveMachineInfoAsync();
        }

        private async Task CheckFirewallStatusAsync()
        {
            await Task.Delay(FirewallCheckDelay);

            if (this.healthProbeService.HealthMasStatus == HealthStatus.Unknown)
            {
                try
                {
                    var firewallManagerType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                    var firewallManager = (dynamic)Activator.CreateInstance(firewallManagerType);
                    bool isFirewallenabled = firewallManager.LocalPolicy.CurrentProfile.FirewallEnabled;

                    if (isFirewallenabled)
                    {
                        this.ShowNotification(
                            Resources.Localized.Get("InstallationApp.FirewallIsEnabledOnThisTerminal"),
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

        private async Task RetrieveMachineInfoAsync()
        {
            this.logger.Info($"Status of machine automation service is '{this.healthProbeService.HealthMasStatus}', WMS service is '{this.healthProbeService.HealthWmsStatus}'.");
            var isWmsEnabled = this.machineModeService.IsWmsEnabled;

            switch (this.healthProbeService.HealthMasStatus)
            {
                case HealthStatus.Initialized:
                case HealthStatus.Initializing:
                    this.ShowNotification(Resources.Localized.Get("LoadLogin.ServiceInitialization"));

                    break;

                case HealthStatus.Healthy:
                case HealthStatus.Degraded:

                    this.ShowNotification(Resources.Localized.Get("LoadLogin.ConnectionEstablished"), Services.Models.NotificationSeverity.Success);

                    try
                    {
                        await this.bayManager.InitializeAsync();
                        var machineIdentity = this.bayManager.Identity;

                        if (this.ServiceHealthStatus != this.healthProbeService.HealthMasStatus
                            && (!isWmsEnabled || this.healthProbeService.HealthWmsStatus == HealthStatus.Unhealthy)
                            )
                        {
                            this.ServiceHealthStatus = this.healthProbeService.HealthMasStatus;
                            this.NavigateToLoginPage(machineIdentity);
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    catch (Exception ex2)
                    {
                        this.logger.Debug(ex2.Message);
                    }

                    break;

                case HealthStatus.Unhealthy:

                    this.ShowNotification(Resources.Localized.Get("LoadLogin.ConnectionNotPossible"), Services.Models.NotificationSeverity.Error);
                    break;
            }
        }

        #endregion
    }
}
