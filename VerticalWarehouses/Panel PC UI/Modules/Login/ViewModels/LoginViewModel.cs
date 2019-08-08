using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    public class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly ISessionService sessionService;

        private readonly IBayManager bayManager;

        private readonly IIdentityMachineService identityMachineService;

        private readonly IHealthProbeService healthProbeService;

        private ICommand loginCommand;

        private ICommand switchOffCommand;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            ISessionService sessionService,
            IBayManager bayManager,
            IIdentityMachineService identityMachineService,
            IHealthProbeService healthProbeService)
            : base(PresentationMode.Login)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            if (sessionService == null)
            {
                throw new ArgumentNullException(nameof(sessionService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (identityMachineService == null)
            {
                throw new ArgumentNullException(nameof(identityMachineService));
            }

            if (healthProbeService == null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
            this.bayManager = bayManager;
            this.identityMachineService = identityMachineService;
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

        public override void OnNavigated()
        {
            base.OnNavigated();
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
            if (this.healthProbeService.HealthStatus == HealthStatus.Healthy
                ||
                this.healthProbeService.HealthStatus == HealthStatus.Degraded)
            {
                await this.bayManager.InitializeAsync();
                this.MachineIdentity = this.bayManager.Identity;
                this.IsLoginAllowed = true;
            }
            else if (this.healthProbeService.HealthStatus == HealthStatus.Unhealthy)
            {
                this.IsLoginAllowed = false;

                this.EventAggregator
                    .GetEvent<PresentationChangedPubSubEvent>()
                    .Publish(new PresentationChangedMessage("Impossibile connettersi al servizio di automazione.")); // TODO move to resources

                //this.NavigationService.Appear(nameof(M)) // move to navigation
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

        public ICommand SwitchOffCommand =>
            this.switchOffCommand
            ??
            (this.switchOffCommand = new DelegateCommand(() => this.ExecuteSwitchOffCommand()));

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

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.RetrieveMachineInfoAsync();
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

        private void ExecuteSwitchOffCommand()
        {
            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.ShowError("Shutting down ...");
            }
        }

        private void LoadInstallerModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Installation));

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.INSTALLATORMENU);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
            finally
            {
            }
        }

        private void LoadOperatorModule()
        {
            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Operator));

                this.NavigationService.Appear(nameof(Utils.Modules.Operator), "TODO"); // TODO
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
            finally
            {
            }
        }
    }
}
