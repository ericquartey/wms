using System;
using System.ComponentModel;
#if !DEBUG
using System.Configuration;
#endif
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

        private readonly INavigationService navigationService;

        private readonly INotificationService notificationService;

        private readonly ISetupModeService setupModeService;

        private IViewModel? activeViewModel;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            INavigationService navigationService,
            IInstallationService installationService,
            ISetupModeService setupModeService,
            INotificationService notificationService)
        {
            if (notificationService is null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

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

            this.setupModeService = setupModeService;
            this.installationService = installationService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;
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

        private bool TrySetCurrentDirectory()
        {
#if !DEBUG
            var installerDirName = ConfigurationManager.AppSettings.GetInstallerDirName();

            var installerLocation = Path.Combine(
                ConfigurationManager.AppSettings.GetUpdateTempPath(),
                installerDirName);

            if (!Directory.Exists(installerLocation))
            {
                this.notificationService.SetErrorMessage($"Folder '{installerLocation}' does not exists.");
                return false;
            }

            Directory.SetCurrentDirectory(installerLocation);

#endif
            return true;
        }

        private async Task StartInstallationAsync()
        {
            if (!this.TrySetCurrentDirectory())
            {
                return;
            }

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
                this.notificationService.SetErrorMessage($"Steps file not found in current directory '{Directory.GetCurrentDirectory()}'.");
                return;
            }

            try
            {
                await this.installationService.DeserializeAsync(sourceFileName);

                await this.NavigateToFirstView(snapshotFileFound);
            }
            catch (Exception ex)
            {
                this.notificationService.SetErrorMessage($"Unable to initialize installation.{Environment.NewLine}{ex.Message}");
            }
        }

        private async Task NavigateToFirstView(bool snapshotFileFound)
        {
            switch (this.setupModeService.Mode)
            {
                case SetupMode.Install:
                case SetupMode.Update:
                case SetupMode.Restore:
                    {
                        if (snapshotFileFound)
                        {
                            var viewModel = new StepsViewModel(
                                Core.Container.GetInstallationService(),
                                NotificationService.GetInstance());

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
            }
        }

        #endregion
    }
}
