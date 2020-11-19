using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Properties;
using Ferretto.VW.Installer.Services;
using Microsoft.Win32;

#nullable enable

namespace Ferretto.VW.Installer.ViewModels
{
    internal sealed class RoleSelectionViewModel : BindableBase, IOperationResult, IViewModel
    {
        #region Fields

        private readonly IInstallationService installationService;

        private readonly Command loadConfigurationCommand;

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineConfigurationService machineConfigurationService;

        private readonly Command navigateToNextPageCommand;

        private readonly INavigationService navigationService;

        private readonly INotificationService notificationService;

        private readonly Command openFileCommand;

        private readonly ISetupModeService setupModeService;

        private bool isLoadingMachineConfiguration;

        private bool isMasterRoleSelected = true;

        private string? machineConfigurationFileName;

        private string? serviceUrl;

        private string? serviceVersion;

        private string? uiVersion;

        #endregion

        #region Constructors

        public RoleSelectionViewModel(
            INavigationService navigationService,
            IInstallationService installationService,
            INotificationService statusBarService,
            ISetupModeService setupModeService,
            IMachineConfigurationService machineConfigurationService)
        {
            if (setupModeService is null)
            {
                throw new ArgumentNullException(nameof(setupModeService));
            }
            this.navigationService = navigationService;
            this.installationService = installationService;
            this.machineConfigurationService = machineConfigurationService;
            this.notificationService = statusBarService;
            this.setupModeService = setupModeService;

            this.navigateToNextPageCommand = new Command(async () => await this.NavigateToNextPageAsync(), this.CanNavigateToNextPage);
            this.openFileCommand = new Command(
                async () => await this.OpenConfigurationFileAsync(),
                this.CanOpenFile);
            this.loadConfigurationCommand = new Command(
                async () => await this.LoadConfigurationFromServiceAsync(),
                this.CanLoadConfiguration);
        }

        #endregion

        #region Properties

        public bool IsLoadingMachineConfiguration
        {
            get => this.isLoadingMachineConfiguration;
            set => this.SetProperty(ref this.isLoadingMachineConfiguration, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMasterRoleSelected
        {
            get => this.isMasterRoleSelected;
            set => this.SetProperty(ref this.isMasterRoleSelected, value, this.SwitchRoleSelection);
        }

        public bool IsSuccessful { get; private set; }

        public ICommand LoadConfigurationCommand => this.loadConfigurationCommand;

        public string? MachineConfigurationFileName
        {
            get => this.machineConfigurationFileName;
            set => this.SetProperty(ref this.machineConfigurationFileName, value);
        }

        public ICommand NavigateToNextPageCommand => this.navigateToNextPageCommand;

        public ICommand OpenFileCommand => this.openFileCommand;

        public string? ServiceUrl
        {
            get => this.serviceUrl;
            set => this.SetProperty(ref this.serviceUrl, value, () =>
            {
                this.machineConfigurationService.ClearConfiguration();
                this.RaiseCanExecuteChanged();
            });
        }

        public string? ServiceVersion
        {
            get => this.serviceVersion;
            set => this.SetProperty(ref this.serviceVersion, value);
        }

        public string? UiVersion
        {
            get => this.uiVersion;
            set => this.SetProperty(ref this.uiVersion, value);
        }

        #endregion

        #region Methods

        public Task OnAppearAsync()
        {
            this.ServiceUrl = ConfigurationManager.AppSettings.GetInstallDefaultMasUrl().ToString();

            this.UiVersion = this.installationService.PanelPcVersion;
            this.ServiceVersion = this.installationService.MasVersion;

            this.notificationService.ClearMessage();

            return Task.CompletedTask;
        }

        public Task OnDisappearAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        public async Task OpenConfigurationFileAsync()
        {
            this.notificationService.ClearMessage();

            this.logger.Debug("User is selecting a vertimag configuration file ...");

            this.IsLoadingMachineConfiguration = true;

            var dialog = new OpenFileDialog()
            {
                DefaultExt = "*.json",
                InitialDirectory = (this.setupModeService.Mode == SetupMode.Update) ? $"d:\\{ConfigurationManager.AppSettings.GetMasDirName()}\\Configuration" : Directory.GetCurrentDirectory(),
                Filter = $"{Resources.VertimagConfigurationFiles}|*.json|{Resources.AllFiles}|*.*",
                Title = Resources.SelectVertimagConfigurationFile,
                Multiselect = false
            };

            var actionConfirmed = dialog.ShowDialog();
            if (actionConfirmed.HasValue
                &&
                actionConfirmed.Value
                &&
                dialog.FileName != null)
            {
                this.logger.Debug($"User selected vertimag configuration file '{dialog.FileName}'.");

                try
                {
                    await this.machineConfigurationService.LoadFromFileAsync(dialog.FileName);

                    var configurationFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "..",
                        ConfigurationManager.AppSettings.GetMasDirName(),
                        "Configuration",
                        "vertimag-configuration.json");

                    await this.machineConfigurationService.SaveToFileAsync(configurationFilePath);

                    this.MachineConfigurationFileName = dialog.FileName;

                    this.notificationService.SetMessage("Configuration file loaded.");
                }
                catch (Exception ex)
                {
                    this.notificationService.SetErrorMessage($"Unable to load configuration from file.{Environment.NewLine}{ex.Message}");
                    this.MachineConfigurationFileName = null;
                }
                finally
                {
                    this.IsLoadingMachineConfiguration = false;
                }
            }
            else
            {
                this.logger.Debug("User canceled Vertimag configuration file selection.");

                this.machineConfigurationService.ClearConfiguration();
                this.MachineConfigurationFileName = null;
                this.IsLoadingMachineConfiguration = false;
            }
        }

        private bool CanLoadConfiguration() =>
            !string.IsNullOrWhiteSpace(this.ServiceUrl)
            &&
            !this.IsLoadingMachineConfiguration;

        private bool CanNavigateToNextPage() =>
            !this.IsLoadingMachineConfiguration
            &&
            this.machineConfigurationService.Configuration != null
            &&
            Uri.TryCreate(this.serviceUrl, UriKind.Absolute, out var _);

        private bool CanOpenFile()
        {
            return !this.IsLoadingMachineConfiguration;
        }

        private async Task LoadConfigurationFromServiceAsync()
        {
            if (this.serviceUrl is null)
            {
                this.logger.Warn("Cannot load configuration from web service because the ServiceUrl is null");
                return;
            }

            try
            {
                this.IsLoadingMachineConfiguration = true;
                await this.machineConfigurationService.LoadFromWebServiceAsync(new Uri(this.serviceUrl));

                this.notificationService.SetMessage("Configuration loaded.");
            }
            catch (Exception ex)
            {
                this.notificationService.SetErrorMessage($"Unable to load configuration from web service.{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                this.IsLoadingMachineConfiguration = false;
            }
        }

        private async Task NavigateToNextPageAsync()
        {
            if (this.ServiceUrl is null)
            {
                this.logger.Warn("Cannot navigate to next page because the ServiceUrl is null");

                return;
            }

            this.installationService.SetConfiguration(new Uri(this.ServiceUrl));

            var viewModel = new BaySelectionViewModel(
                Container.GetInstallationService(),
                NavigationService.GetInstance(),
                NotificationService.GetInstance(),
                Container.GetMachineConfigurationService());

            await this.navigationService.NavigateToAsync(viewModel);
        }

        private void RaiseCanExecuteChanged()
        {
            this.loadConfigurationCommand.RaiseCanExecuteChanged();
            this.navigateToNextPageCommand.RaiseCanExecuteChanged();
            this.openFileCommand.RaiseCanExecuteChanged();
        }

        private void SwitchRoleSelection()
        {
            this.machineConfigurationService.ClearConfiguration();

            this.MachineConfigurationFileName = null;

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
