using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    public class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IHealthProbeService healthProbeService;

        private SubscriptionToken subscriptionToken;

        private DelegateCommand loginCommand;

        private HealthStatus serviceHealthStatus;

        #endregion

        public override EnableMask EnableMask => EnableMask.None;

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(PresentationMode.Login)
        {
            if (authenticationService is null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            if (healthProbeService is null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.authenticationService = authenticationService;
            this.healthProbeService = healthProbeService;

            this.BayNumber = bayManager.Bay.Number;
            this.ServiceHealthStatus = this.healthProbeService.HealthStatus;

            this.subscriptionToken = this.healthProbeService.HealthStatusChanged.Subscribe(
                this.OnHealthStatusChanged,
                ThreadOption.UIThread,
                false);

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "password",
            };
#else
            this.UserLogin = new UserLogin();
#endif
        }

        public override void Disappear()
        {
            base.Disappear();

            this.UserLogin.IsValidationEnabled = true;
            this.UserLogin.Password = null;

            if (this.subscriptionToken != null)
            {
                this.healthProbeService.HealthStatusChanged.Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        #endregion

        #region Properties

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(
                async () => await this.ExecuteLoginCommandAsync(),
                this.CanExecuteLogin));

        private bool CanExecuteLogin()
        {
            return this.machineIdentity != null
                &&
                string.IsNullOrEmpty(this.UserLogin.Error)
                &&
                (this.ServiceHealthStatus == HealthStatus.Healthy || this.ServiceHealthStatus == HealthStatus.Degraded);
        }

        public UserLogin UserLogin { get; }

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set
            {
                if (this.SetProperty(ref this.machineIdentity, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.loginCommand?.RaiseCanExecuteChanged();
        }

        public int BayNumber { get; }

        public HealthStatus ServiceHealthStatus
        {
            get => this.serviceHealthStatus;
            set
            {
                if (this.SetProperty(ref this.serviceHealthStatus, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            this.ServiceHealthStatus = e.HealthStatus;

            if (this.ServiceHealthStatus == HealthStatus.Degraded
                ||
                this.ServiceHealthStatus == HealthStatus.Healthy)
            {
                this.ShowNotification("Connessione ai servizi ristabilita");
            }
        }

        private MachineIdentity machineIdentity;

        #endregion

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
            }
        }

        private async Task ExecuteLoginCommandAsync()
        {
            this.ShowNotification(string.Empty);

            this.UserLogin.IsValidationEnabled = true;
            if (!string.IsNullOrEmpty(this.UserLogin.Error))
            {
                this.ShowNotification(this.UserLogin.Error, Services.Models.NotificationSeverity.Error);
                return;
            }

            this.NavigationService.IsBusy = true;

            var claims = await this.authenticationService.LogInAsync(
               this.UserLogin.UserName,
               this.UserLogin.Password);

            this.NavigationService.IsBusy = false;

            if (claims != null)
            {
                if (claims.AccessLevel == UserAccessLevel.SuperUser)
                {
                    this.NavigateToInstallerMainView();
                }
                else
                {
                    this.NavigateToOperatorMainView();
                }
            }
            else
            {
                this.ShowNotification(Resources.Errors.UserLogin_InvalidCredentials, Services.Models.NotificationSeverity.Error);
            }
        }

        private void NavigateToInstallerMainView()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.INSTALLATORMENU,
                data: null,
                trackCurrentView: true);
        }

        private void NavigateToOperatorMainView()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
               "TODO",/// Utils.Modules.Operator,
                data: null,
                trackCurrentView: true);
        }
    }
}
