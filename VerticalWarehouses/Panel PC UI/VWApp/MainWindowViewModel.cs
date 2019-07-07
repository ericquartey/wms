using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Services;
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

        public IUnityContainer Container;

        private readonly IAuthenticationService authenticationService;

        private readonly IEventAggregator eventAggregator;

        private readonly ISessionService sessionService;

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
            ISessionService sessionService)
        {
            this.eventAggregator = eventAggregator;
            this.themeService = themeService;
            this.authenticationService = authenticationService;
            this.sessionService = sessionService;
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
#if DEBUG
            = new UserLogin { UserName = "installer", Password = "password" };

#else
            = new UserLogin();

#endif

        #endregion

        #region Methods

        public void InitializeViewModel(IUnityContainer container)
        {
            this.Container = container;
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
                var moduleManager = this.Container.Resolve<IModuleManager>();
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
                var moduleManager = this.Container.Resolve<IModuleManager>();
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
