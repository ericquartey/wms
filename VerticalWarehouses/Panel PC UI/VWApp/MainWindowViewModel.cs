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

namespace Ferretto.VW.App
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private IUnityContainer container;

        private readonly IAuthenticationService authenticationService;

        private readonly IEventAggregator eventAggregator;

        private readonly ISessionService sessionService;

        private readonly IStatusMessageService statusMessageService;

        private readonly IBayManager bayManager;

        private readonly IMachineIdentityService identityService;

        private readonly IHealthProbeService healthProbeService;

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
            IStatusMessageService statusMessageService,
            IBayManager bayManager,
            IMachineIdentityService identityService,
            IHealthProbeService healthProbeService)
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

            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (identityService == null)
            {
                throw new ArgumentNullException(nameof(identityService));
            }

            if (healthProbeService == null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            this.eventAggregator = eventAggregator;
            this.themeService = themeService;
            this.moduleManager = moduleManager;
            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
            this.statusMessageService = statusMessageService;
            this.bayManager = bayManager;
            this.identityService = identityService;
            this.healthProbeService = healthProbeService;

            this.healthProbeService.SubscribeOnHealthStatusChanged(async (e) => await this.OnHealthStatusChanged(e));

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

        private async Task OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            await this.RetrieveMachineInfo();
        }

        private async Task RetrieveMachineInfo()
        {
            if (this.healthProbeService.HealthStatus == HealthStatus.Healthy
                ||
                this.healthProbeService.HealthStatus == HealthStatus.Degraded)
            {
                this.ErrorMessage = null;
                await this.bayManager.InitializeAsync();
                this.Machine = this.bayManager.Identity;
                this.IsLoginAllowed = true;
            }
            else if (this.healthProbeService.HealthStatus == HealthStatus.Unhealthy)
            {
                this.IsLoginAllowed = false;
                this.ErrorMessage = "Impossibile connettersi al servizio di automazione.";
                if (Application.Current is App app)
                {
                    if (app.OperatorAppMainWindowInstance?.IsVisible == true)
                    {
                        app.OperatorAppMainWindowInstance.Hide();
                    }

                    if (app.InstallationAppMainWindowInstance?.IsVisible == true)
                    {
                        app.InstallationAppMainWindowInstance.Hide();
                    }
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

        private MachineIdentity machineInfo;

        private bool isMachineIdentityAvailable;

        private bool isLoginAllowed;

        #endregion

        #region Methods

        public async Task InitializeViewModelAsync(IUnityContainer container)
        {
            this.container = container;

            await this.RetrieveMachineInfo();
        }

        public bool IsMachineIdentityAvailable
        {
            get => this.isMachineIdentityAvailable;
            set => this.SetProperty(ref this.isMachineIdentityAvailable, value);
        }

        public bool IsLoginAllowed
        {
            get => this.isLoginAllowed;
            set => this.SetProperty(ref this.isLoginAllowed, value);
        }

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
                this.ErrorMessage = Resources.Errors.UserLogin_InvalidCredentials;
            }

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
