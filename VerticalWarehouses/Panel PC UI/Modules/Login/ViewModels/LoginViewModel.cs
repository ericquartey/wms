using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
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

        private ICommand loginCommand;

        private ICommand switchOffCommand;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            ISessionService sessionService,
            IBayManager bayManager,
            IIdentityMachineService identityMachineService) : base(PresentationMode.Login)
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

            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
            this.bayManager = bayManager;
            this.identityMachineService = identityMachineService;

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "operator",
                Password = "password",
            };
#else
            this.UserLogin = new UserLogin();
#endif

            this.HACK_InitialiseHubOperator();
        }

        public override void OnNavigated()
        {
            base.OnNavigated();
            this.NavigationService.SetBusy(true);
        }

        private async Task OnHubConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (!e.IsConnected)
            {
                this.ShowError("Connection to Machine Automation Service lost.");
            }
            else
            {
                try
                {
                    await this.bayManager.InitializeAsync();
                    this.Machine = this.bayManager.Identity;
                    this.ShowError(string.Empty);
                }
                catch
                {
                    this.ShowError("Unable to retrieve machine info.");
                }
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
            return this.machineInfo != null;
        }

        public ICommand SwitchOffCommand =>
            this.switchOffCommand
            ??
            (this.switchOffCommand = new DelegateCommand(() => this.ExecuteSwitchOffCommand()));

        public UserLogin UserLogin { get; }

        public MachineIdentity Machine
        {
            get => this.machineInfo;
            set
            {
                if (this.SetProperty(ref this.machineInfo, value))
                {
                    ((DelegateCommand)this.LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private readonly IOperatorHubClient operatorHubClient = ServiceLocator.Current.GetInstance<IOperatorHubClient>();

        private MachineIdentity machineInfo;

        #endregion

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.HACK_InitialiseHubOperator();
        }

        public void HACK_InitialiseHubOperator()
        {
            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionStatusChanged(sender, e);
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

            this.IsBusy = true;

            var claims = await this.authenticationService.LogInAsync(
               this.UserLogin.UserName,
               this.UserLogin.Password);

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

            this.IsBusy = false;
        }

        private void ExecuteSwitchOffCommand()
        {
            this.IsBusy = true;
            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.ShowError("Shutting down ...");
            }
        }

        private void LoadInstallerModule()
        {
            this.IsBusy = true;

            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Installation));

                this.IsBusy = false;

                this.NavigationService.ShowInstallation();
            }
            catch (Exception ex)
            {
                this.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void LoadOperatorModule()
        {
            this.IsBusy = true;

            try
            {
                this.NavigationService.LoadModule(nameof(Utils.Modules.Operator));

                this.IsBusy = false;

                this.NavigationService.ShowInstallation();
            }
            catch (Exception ex)
            {
                this.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}
