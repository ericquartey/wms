using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    public class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly IHealthProbeService healthProbeService;

        private ICommand loginCommand;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            IBayManager bayManager,
            IHealthProbeService healthProbeService)
            : base(PresentationMode.Login)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (healthProbeService == null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            this.authenticationService = authenticationService;
            this.bayManager = bayManager;
            this.healthProbeService = healthProbeService;

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "password",
            };
#else
            this.UserLogin = new UserLogin();
#endif

            this.healthProbeService.SubscribeOnHealthStatusChanged(async (e) => await this.OnHealthStatusChanged(e));
        }

        private async Task OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            await this.RetrieveMachineInfoAsync();
        }

        public bool IsLoginAllowed
        {
            get => this.isLoginAllowed;
            set => this.SetProperty(ref this.isLoginAllowed, value);
        }

        private bool isLoginAllowed;

        private async Task RetrieveMachineInfoAsync()
        {
            switch (this.healthProbeService.HealthStatus)
            {
                case HealthStatus.Healthy:
                case HealthStatus.Degraded:
                    await this.bayManager.InitializeAsync();
                    this.MachineIdentity = this.bayManager.Identity;
                    this.IsLoginAllowed = true;
                    break;

                case HealthStatus.Unhealthy:
                    this.IsLoginAllowed = false;

                    this.EventAggregator
                        .GetEvent<PresentationChangedPubSubEvent>()
                        .Publish(new PresentationChangedMessage("Impossibile connettersi al servizio di automazione.")); // TODO move to resources

                    //this.NavigationService.Appear(nameof(M)) // move to navigation
                    break;
            }
        }

        #endregion

        #region Properties

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(async () => await this.ExecuteLoginCommandAsync(), this.CanExecuteLogin));

        private bool CanExecuteLogin()
        {
            return this.machineIdentity != null
                &&
                string.IsNullOrEmpty(this.UserLogin.Error);
        }

        public UserLogin UserLogin { get; }

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set
            {
                if (this.SetProperty(ref this.machineIdentity, value))
                {
                    ((DelegateCommand)this.LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private MachineIdentity machineIdentity;

        #endregion

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            await this.RetrieveMachineInfoAsync();
        }

        private async Task ExecuteLoginCommandAsync()
        {
            this.ShowError(string.Empty);

            this.UserLogin.IsValidationEnabled = true;
            if (!string.IsNullOrEmpty(this.UserLogin.Error))
            {
                this.ShowError(this.UserLogin.Error);
                return;
            }

            this.NavigationService.SetBusy(true);

            var claims = await this.authenticationService.LogInAsync(
               this.UserLogin.UserName,
               this.UserLogin.Password);

            this.NavigationService.SetBusy(false);

            if (claims != null)
            {
                if (claims.AccessLevel == UserAccessLevel.SuperUser)
                {
                    this.LoadInstallerModule();
                }
                else
                {
                    this.LoadOperatorModule();
                }
            }
            else
            {
                this.ShowError(Resources.Errors.UserLogin_InvalidCredentials);
            }
        }

        private void LoadInstallerModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Installation));

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.INSTALLATORMENU, true);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void LoadOperatorModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Operator));

                this.NavigationService.Appear(nameof(Utils.Modules.Operator), "TODO", true); // TODO
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }
    }
}
