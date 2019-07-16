using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;
using ConnectionStatusChangedEventArgs = Ferretto.VW.MAS.AutomationService.Contracts.ConnectionStatusChangedEventArgs;
using IOperatorHubClient = Ferretto.VW.MAS.AutomationService.Contracts.IOperatorHubClient;

namespace Ferretto.VW.App
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private IUnityContainer container;

        private readonly IAuthenticationService authenticationService;

        private readonly IEventAggregator eventAggregator;

        private readonly ISessionService sessionService;

        private readonly IMachineProvider machineProvider;

        private readonly IThemeService themeService;

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
            ISessionService sessionService,
            IMachineProvider machineProvider)
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

            if (sessionService == null)
            {
                throw new ArgumentNullException(nameof(sessionService));
            }

            if (machineProvider == null)
            {
                throw new ArgumentNullException(nameof(machineProvider));
            }

            this.eventAggregator = eventAggregator;
            this.themeService = themeService;
            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
            this.machineProvider = machineProvider;

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

        private void OperatorHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                this.ErrorMessage = "Machine Automation Service connected!!!!!!!!!!!!!!!!!!!!!!!!";
            }
            else
            {
                this.ErrorMessage = "Connection to Machine Automation Service lost.";
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

        public async Task InitializeViewModelAsync(IUnityContainer container)
        {
            this.container = container;

            try
            {
                this.Machine = await this.machineProvider.GetIdentityAsync();
            }
            catch
            {
            }
        }

        public void HACK_InitialiseHubOperator()
        {
            this.operatorHubClient = this.container.Resolve<IOperatorHubClient>();

            this.operatorHubClient.ConnectionStatusChanged += this.OperatorHubClient_ConnectionStatusChanged;
            if (!this.operatorHubClient.IsConnected)
            {
                this.ErrorMessage = "Machine Automation Service unavailable.";
            }
        }

        public bool IsMachineIdentityAvailable
        { get => this.isMachineIdentityAvailable; set => this.SetProperty(ref this.isMachineIdentityAvailable, value); }

        private async Task ExecuteLoginCommandAsync()
        {
            this.ErrorMessage = null;

            this.UserLogin.IsValidationEnabled = true;
            if (!string.IsNullOrEmpty(this.UserLogin.Error))
            {
                this.ErrorMessage = this.UserLogin.Error;
                return;
            }

            this.IsBusy = true;

            var loginSuccessful = await this.authenticationService.LogInAsync(
                this.UserLogin.UserName,
                this.UserLogin.Password);

            if (!loginSuccessful)
            {
                this.ErrorMessage = Resources.Errors.UserLogin_InvalidCredentials;
                this.IsBusy = false;
                return;
            }

            switch (this.UserLogin.UserName.ToUpperInvariant())
            {
                case "INSTALLER":
                    await this.LoadInstallerModuleAsync();
                    break;

                case "OPERATOR":
                    await this.LoadOperatorModuleAsync();
                    break;
            }

            this.IsBusy = false;
        }

        private void ExecuteSwitchOffCommand()
        {
            this.IsBusy = true;
            var requestAccepted = this.sessionService.Shutdown();
            if (requestAccepted)
            {
                this.ErrorMessage = "Shutting down";
            }
        }

        private async Task LoadInstallerModuleAsync()
        {
            this.IsBusy = true;

            try
            {
                var moduleManager = this.container.Resolve<IModuleManager>();
                moduleManager.LoadModule("Installation");

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

        private async Task LoadOperatorModuleAsync()
        {
            this.IsBusy = true;

            try
            {
                var moduleManager = this.container.Resolve<IModuleManager>();
                moduleManager.LoadModule("Operator");

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
