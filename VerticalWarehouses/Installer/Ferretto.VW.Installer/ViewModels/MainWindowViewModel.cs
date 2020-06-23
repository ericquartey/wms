using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Services;

#nullable enable

namespace Ferretto.VW.Installer.ViewModels
{
    public class MainWindowViewModel : BindableBase, IViewModel
    {
        #region Fields

        private const string StepsDefinitionFileName = "steps.json";

        private const string StepsSnapshotFileName = "steps-snapshot.json";

        private readonly ICommand closeCommand;

        private readonly IInstallationService installationService;

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService;

        private readonly ISetupModeService setupModeService;

        private readonly INotificationService statusBarService;

        private IViewModel? activeViewModel;

        private string? errorMessage;

        private bool hasError;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            INavigationService navigationService,
            IInstallationService installationService,
            ISetupModeService setupModeService,
            INotificationService statusBarService)
        {
            if (navigationService is null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (installationService is null)
            {
                throw new ArgumentNullException(nameof(installationService));
            }

            if (setupModeService is null)
            {
                throw new ArgumentNullException(nameof(setupModeService));
            }

            if (statusBarService is null)
            {
                throw new ArgumentNullException(nameof(statusBarService));
            }

            this.setupModeService = setupModeService;
            this.installationService = installationService;
            this.navigationService = navigationService;
            this.statusBarService = statusBarService;
            this.navigationService.PropertyChanged += this.OnNavigationServicePropertyChanged;

            this.closeCommand = new Command(this.Close);
        }

        #endregion

        #region Properties

        public IViewModel? ActiveViewModel
        {
            get => this.activeViewModel;
            set => this.SetProperty(ref this.activeViewModel, value);
        }

        public ICommand CloseCommand => this.closeCommand;

        public string? ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        public bool HasError
        {
            get => this.hasError;
            set => this.SetProperty(ref this.hasError, value);
        }

        #endregion

        #region Methods

        public async Task OnAppearAsync()
        {
            await this.StartInstallationAsync();
        }

        public Task OnDisappearAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        private void Close()
        {
            Application.Current.Shutdown((this.ActiveViewModel as IOperationResult)?.IsSuccessful == true ? 0 : -1);
        }

        private void OnNavigationServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(this.navigationService.ActiveViewModel))
            {
                return;
            }

            this.ActiveViewModel = this.navigationService.ActiveViewModel;
        }

        private void OnStatusBarServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShowError(string message)
        {
            this.logger.Error(message);
            this.HasError = true;
            this.ErrorMessage = message;
        }

        private async Task StartInstallationAsync()
        {
            var installerDirName = ConfigurationManager.AppSettings.GetInstallerDirName();

            var installerLocation = Path.Combine(
                ConfigurationManager.AppSettings.GetUpdateTempPath(),
                installerDirName);

#if !DEBUG
            if (!Directory.Exists(installerLocation))
            {
                this.ShowError($"Folder '{installerLocation}' does not exists.");
                return;
            }

            Directory.SetCurrentDirectory(installerLocation);
#endif
            var stepsFileFound = false;
            var snapshotFileFound = false;
            var sourceFileName = StepsSnapshotFileName;

            if (File.Exists(StepsSnapshotFileName))
            {
                stepsFileFound = true;
                snapshotFileFound = true;
            }
            else if (File.Exists(StepsDefinitionFileName))
            {
                sourceFileName = StepsDefinitionFileName;
                stepsFileFound = true;
            }

            if (!stepsFileFound)
            {
                this.ShowError($"Steps file not found in folder '{Directory.GetCurrentDirectory()}'.");
                return;
            }

            try
            {
                await this.installationService.DeserializeAsync(sourceFileName);

                switch (this.setupModeService.Mode)
                {
                    case SetupMode.Install:
                        {
                            if(snapshotFileFound)
                            {
                                var viewModel = new StepsViewModel(
                                    Core.Container.GetInstallationService());

                                await this.navigationService.NavigateToAsync(viewModel);
                            }
                            else
                            {
                                var viewModel = new RoleSelectionViewModel(
                                    NavigationService.GetInstance(),
                                    Core.Container.GetInstallationService(),
                                    NotificationService.GetInstance(),
                                    Core.Container.GetMachineConfigurationService());

                                await this.navigationService.NavigateToAsync(viewModel);

                            }

                            break;
                        }

                    case SetupMode.Update:
                    case SetupMode.Restore:
                        {
                            var viewModel = new StepsViewModel(
                                Core.Container.GetInstallationService());

                            await this.navigationService.NavigateToAsync(viewModel);

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                this.ShowError($"Unable to initialize installation: {ex.Message}");
            }
        }

        #endregion
    }
}
