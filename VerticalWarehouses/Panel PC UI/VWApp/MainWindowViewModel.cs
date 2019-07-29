using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;
using ConnectionStatusChangedEventArgs = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs.ConnectionStatusChangedEventArgs;
using IOperatorHubClient = Ferretto.VW.MAS.AutomationService.Contracts.Hubs.IOperatorHubClient;

namespace Ferretto.VW.App
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private IUnityContainer container;

        private readonly IAuthenticationService authenticationService;

        private readonly IEventAggregator eventAggregator;

        private readonly ISessionService sessionService;

        private readonly IBayManager bayManager;

        private readonly IIdentityService identityService;

        private readonly IThemeService themeService;

        private readonly IModuleManager moduleManager;

        private bool isBusy;

        private ICommand loginCommand;

        private string errorMessage;

        private ICommand switchOffCommand;

        private ICommand toggleThemeCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IAuthenticationService authenticationService,
            IThemeService themeService,
            IModuleManager moduleManager,
            ISessionService sessionService,
            IBayManager bayManager,
            IIdentityService identityService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (authenticationService == null)
            {
                throw new ArgumentNullException(nameof(authenticationService));
            }

            if (themeService == null)
            {
                throw new ArgumentNullException(nameof(themeService));
            }

            if (moduleManager == null)
            {
                throw new ArgumentNullException(nameof(moduleManager));
            }

            if (sessionService == null)
            {
                throw new ArgumentNullException(nameof(sessionService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (identityService == null)
            {
                throw new ArgumentNullException(nameof(identityService));
            }

            this.eventAggregator = eventAggregator;
            this.themeService = themeService;
            this.moduleManager = moduleManager;
            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
            this.bayManager = bayManager;
            this.identityService = identityService;

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

        private async Task OnHubConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (!e.IsConnected)
            {
                this.ErrorMessage = "Connection to Machine Automation Service lost.";
            }
            else
            {
                try
                {
                    await this.bayManager.InitializeAsync();
                    this.Machine = this.bayManager.Identity;

                    this.ErrorMessage = null;
                }
                catch
                {
                    this.ErrorMessage = "Unable to retrieve machine info.";
                }
            }
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(async () => await this.ExecuteLoginCommandAsync()));

        public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

        public ICommand SwitchOffCommand =>
            this.switchOffCommand
            ??
            (this.switchOffCommand = new DelegateCommand(() => this.ExecuteSwitchOffCommand()));

        public ICommand ToggleThemeCommand =>
            this.toggleThemeCommand
            ??
            (this.toggleThemeCommand = new DelegateCommand(() => this.ToggleTheme()));

        public UserLogin UserLogin { get; }

        public MachineIdentity Machine
        {
            get => this.machineInfo;
            set
            {
                if (this.SetProperty(ref this.machineInfo, value))
                {
                    this.IsMachineIdentityAvailable = value != null;
                }
            }
        }

        public IOperatorHubClient operatorHubClient;

        private MachineIdentity machineInfo;

        private bool isMachineIdentityAvailable;

        #endregion

        #region Methods

        public Task InitializeViewModelAsync(IUnityContainer container)
        {
            this.container = container;

            return Task.CompletedTask;
        }

        public void HACK_InitialiseHubOperator()
        {
            this.operatorHubClient = this.container.Resolve<IOperatorHubClient>();

            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionStatusChanged(sender, e);
        }

        public bool IsMachineIdentityAvailable
        {
            get => this.isMachineIdentityAvailable;
            set => this.SetProperty(ref this.isMachineIdentityAvailable, value);
        }

        private async Task ExecuteLoginCommandAsync()
        {
            this.ErrorMessage = null;

            //TEMP: I have commented these code lines in order to use the AS without connection of WMS server
            // Start always the Installer application

            //this.UserLogin.IsValidationEnabled = true;
            //if (!string.IsNullOrEmpty(this.UserLogin.Error))
            //{
            //    this.ErrorMessage = this.UserLogin.Error;
            //    return;
            //}

            //this.IsBusy = true;

            //var claims = await this.authenticationService.LogInAsync(
            //   this.UserLogin.UserName,
            //   this.UserLogin.Password);

            //if (claims != null)
            //{
            //    if (claims.AccessLevel == UserAccessLevel.SuperUser)
            //    {
            //        this.LoadInstallerModule();
            //    }
            //    else
            //    {
            //        this.LoadOperatorModule();
            //    }
            //}
            //else
            //{
            //    this.ErrorMessage = Resources.Errors.UserLogin_InvalidCredentials;
            //}

            this.LoadInstallerModule();

            this.IsBusy = false;
        }

        private void ExecuteSwitchOffCommand()
        {
            this.IsBusy = true;
            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.ErrorMessage = "Shutting down ...";
            }
        }

        private void LoadInstallerModule()
        {
            this.IsBusy = true;

            try
            {
                this.moduleManager.LoadModule("Installation");

                this.IsBusy = false;

                (Application.Current as App)?.InstallationAppMainWindowInstance.Show();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error: {ex.Message}";
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
                this.moduleManager.LoadModule("Operator");

                this.IsBusy = false;

                (Application.Current as App)?.OperatorAppMainWindowInstance.Show();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void ToggleTheme()
        {
            this.themeService.ApplyTheme(
                this.themeService.ActiveTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light);

            this.RaisePropertyChanged(nameof(this.IsDarkThemeActive));
        }

        #endregion
    }
}
