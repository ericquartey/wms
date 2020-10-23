using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Services;
using Ferretto.VW.MAS.DataModels;

#nullable enable

namespace Ferretto.VW.Installer.ViewModels
{
    public class BaySelectionViewModel : BindableBase, IOperationResult, IViewModel
    {
        #region Fields

        private const string AutomationServiceUrlKey = "AutomationService:Url";

        private const string BayNumberKey = "BayNumber";

        private const string TelemetryServiceHubsKey = "TelemetryService:Hubs:Path";

        private const string TelemetryServiceUrlKey = "TelemetryService:Url";

        private readonly Command goBackCommand;

        private readonly IInstallationService installationService;

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineConfigurationService machineConfigurationService;

        private readonly Command navigateToNextPageCommand;

        private readonly INavigationService navigationService;

        private readonly INotificationService notificationService;

        private bool isSuccessful;

        private Bay? selectedBay;

        #endregion

        #region Constructors

        public BaySelectionViewModel(
            IInstallationService installationService,
            INavigationService navigationService,
            INotificationService notificationService,
            IMachineConfigurationService machineConfigurationService)
        {
            this.installationService = installationService;
            this.navigationService = navigationService;
            this.machineConfigurationService = machineConfigurationService;
            this.notificationService = notificationService;

            this.goBackCommand = new Command(this.GoBack);
            this.navigateToNextPageCommand = new Command(
                async () => await this.NavigateToNextPageAsync(),
                this.CanNavigateToNextPage);
        }

        #endregion

        #region Properties

        public IEnumerable<Bay>? Bays => this.machineConfigurationService.Configuration?.Machine?.Bays;

        public ICommand GoBackCommand => this.goBackCommand;

        public bool IsSuccessful => this.isSuccessful;

        public Machine? Machine => this.machineConfigurationService.Configuration?.Machine;

        public ICommand NextCommand => this.navigateToNextPageCommand;

        public Bay? SelectedBay
        {
            get => this.selectedBay;
            set => this.SetProperty(ref this.selectedBay, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public Task OnAppearAsync()
        {
            this.SelectedBay = this.Bays?.FirstOrDefault();

            this.notificationService.ClearMessage();

            return Task.CompletedTask;
        }

        public Task OnDisappearAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        private static string GetBayIpAddress(BayNumber number)
        {
            return number switch
            {
                BayNumber.BayOne => ConfigurationManager.AppSettings.GetInstallBay1Ipaddress(),
                BayNumber.BayTwo => ConfigurationManager.AppSettings.GetInstallBay2Ipaddress(),
                BayNumber.BayThree => ConfigurationManager.AppSettings.GetInstallBay3Ipaddress(),
                _ => throw new InvalidOperationException("The specified bay number is not valid."),
            };
        }

        private bool CanNavigateToNextPage() =>
            this.SelectedBay != null
            &&
            this.installationService.MasUrl != null;

        private void GoBack()
        {
            this.navigationService.NavigateBack();
        }

        private async Task NavigateToNextPageAsync()
        {
            try
            {
                this.SaveInstallerAppConfig();
                this.SavePanelPcAppConfig();

                var viewModel = new StepsViewModel(
                    Container.GetInstallationService(),
                    NotificationService.GetInstance());

                await this.navigationService.NavigateToAsync(viewModel);
                this.isSuccessful = true;
            }
            catch (Exception ex)
            {
                this.notificationService.SetErrorMessage($"Cannot start installation.{Environment.NewLine}{ex.Message}");
            }
        }

        private void PersistConfigurationKey(string keyName, string keyValue, string? applicationPath = null)
        {
            this.logger.Debug($"Saving configuration key '{keyName}'='{keyValue}'. ");

            try
            {
                var config = applicationPath is null
                    ? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                    : ConfigurationManager.OpenExeConfiguration(applicationPath);

                config.AppSettings.Settings.Remove(keyName);
                config.AppSettings.Settings.Add(new KeyValueConfigurationElement(keyName, keyValue));
                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("appSettings");

                this.logger.Debug($"Configuration file '{config.FilePath}' updated. ");
            }
            catch
            {
                this.logger.Error($"Cannot save configuration key '{keyName}'. ");

                throw;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.navigateToNextPageCommand.RaiseCanExecuteChanged();
        }

        private void SaveInstallerAppConfig()
        {
            if (this.SelectedBay is null)
            {
                throw new InvalidOperationException("Cannot save PanelPC configuration because the SelectedBay is null");
            }

            if (this.installationService.MasUrl is null)
            {
                throw new InvalidOperationException("Cannot save PanelPC configuration because the MasUrl is null");
            }

            var bayIpaddress = GetBayIpAddress(this.SelectedBay.Number);
            this.PersistConfigurationKey("Install:Parameter:PpcIpAddress", bayIpaddress);

            if (this.SelectedBay.Number != BayNumber.BayOne)
            {
                this.PersistConfigurationKey("Install:Parameter:StartupType", "Disabled");
            }
        }

        private void SavePanelPcAppConfig()
        {
            if (this.SelectedBay is null)
            {
                throw new InvalidOperationException("Cannot save PanelPC configuration because the SelectedBay is null");
            }

            if (this.installationService.MasUrl is null)
            {
                throw new InvalidOperationException("Cannot save PanelPC configuration because the MasUrl is null");
            }

            var panelPcConfigFileName = System.IO.Path.Combine(
                "..",
                ConfigurationManager.AppSettings.GetPpcDirName(),
                ConfigurationManager.AppSettings.GetPpcFileName());

            this.logger.Debug($"Updating application configuration for executable '{panelPcConfigFileName}' ...");

            var bayNumber = (int)this.SelectedBay.Number;
            var masUrl = this.installationService.MasUrl;

            this.PersistConfigurationKey(BayNumberKey, bayNumber.ToString(), panelPcConfigFileName);
            this.PersistConfigurationKey(AutomationServiceUrlKey, masUrl.ToString(), panelPcConfigFileName);

            var tsUrl = this.installationService.TsUrl;
            if (tsUrl != null)
            {
                this.PersistConfigurationKey(TelemetryServiceUrlKey, tsUrl.ToString(), panelPcConfigFileName);
                this.PersistConfigurationKey(TelemetryServiceHubsKey, tsUrl.ToString() + "/telemetry", panelPcConfigFileName);
            }

            this.logger.Debug($"Application configuration updated.");
        }

        #endregion
    }
}
