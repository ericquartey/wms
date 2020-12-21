using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParameterInverterViewModel : BaseParameterInverterViewModel, ISetVertimagInverterConfiguration
    {
        #region Fields

        private const string RESETDATA = "reset";

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToExport;

        private DelegateCommand goToImport;

        private IEnumerable<FileInfo> importableFiles = Array.Empty<FileInfo>();

        private string importFolderPath;

        private IEnumerable<Inverter> inverters;

        private FileInfo selectedFileConfiguration;

        private Inverter selectedInverter;

        private DelegateCommand setInvertersParamertersCommand;

        private DelegateCommand<Inverter> showInverterParamertersCommand;

        private IEnumerable<Inverter> vertimagInverterConfiguration;

        #endregion

        #region Constructors

        public ParameterInverterViewModel(
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usbWatcher) : base(identityService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToExport =>
               this.goToExport
               ??
               (this.goToExport = new DelegateCommand(
                () => this.ShowExport(), this.CanShowImport));

        public ICommand GoToImport =>
                       this.goToImport
               ??
               (this.goToImport = new DelegateCommand(
                () => this.ShowImport(), this.CanShowImport));

        public IEnumerable<FileInfo> ImportableFiles => this.importableFiles;

        public string ImportFolderPath
        {
            get => this.importFolderPath;
            set => this.SetProperty(ref this.importFolderPath, value);
        }

        public IEnumerable<Inverter> Inverters => this.inverters;

        public FileInfo SelectedFileConfiguration
        {
            get => this.selectedFileConfiguration;
            set => this.SetProperty(ref this.selectedFileConfiguration, value);
        }

        public Inverter SelectedInverter
        {
            get => this.selectedInverter;
            set => this.SetProperty(ref this.selectedInverter, value);
        }

        public ICommand SetInvertersParamertersCommand =>
                   this.setInvertersParamertersCommand
               ??
               (this.setInvertersParamertersCommand = new DelegateCommand(
                async () => await this.SaveAllParametersAsync(), this.CanSave));

        public ICommand ShowInverterParamertersCommand =>
                   this.showInverterParamertersCommand
               ??
               (this.showInverterParamertersCommand = new DelegateCommand<Inverter>(
                   this.ShowInverterParameters));

        public IEnumerable<Inverter> VertimagInverterConfiguration
        {
            get => this.vertimagInverterConfiguration;
            set => this.SetProperty(ref this.vertimagInverterConfiguration, value);
        }

        #endregion

        #region Methods

        public void BackupVertimagInverterConfigurationParameters()
        {
            if (this.selectedFileConfiguration is null)
            {
                return;
            }

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new Modules.Installation.Models.OrderedContractResolver(),
                Converters = new JsonConverter[]
                {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                },
            };

            var json = JsonConvert.SerializeObject(this.VertimagInverterConfiguration, settings);

            var fullPath = this.Filename(this.VertimagInverterConfiguration, new DriveInfo(this.selectedFileConfiguration.DirectoryName), true);
            File.WriteAllText(fullPath, json);
        }

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            if (RESETDATA.Equals(this.Data))
            {
                this.SelectedFileConfiguration = null;
                this.VertimagInverterConfiguration = null;
                this.Data = null;
            }

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();

            await base.OnAppearedAsync();

            this.IsWaitingForResponse = false;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                //add read from inverter

                if (!(this.VertimagInverterConfiguration is null))
                {
                    this.inverters = this.VertimagInverterConfiguration;
                }
                else
                {
                    this.SelectedFileConfiguration = null;
                    this.inverters = await this.machineDevicesWebService.GetInvertersAsync();

                    await this.machineDevicesWebService.ReadAllInvertersAsync(this.inverters);
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Inverters));
                this.IsBusy = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.setInvertersParamertersCommand?.RaiseCanExecuteChanged();
            this.showInverterParamertersCommand?.RaiseCanExecuteChanged();
            this.goToImport?.RaiseCanExecuteChanged();
            this.goToExport?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private bool CanShowImport()
        {
            return this.usbWatcher.Drives.Writable().Any();
        }

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                this.BackupVertimagInverterConfigurationParameters();

                await this.machineDevicesWebService.ProgramAllInvertersAsync(this.vertimagInverterConfiguration);

                this.ShowNotification(Localized.Get("InstallationApp.InvertersProgrammingStarted"), Services.Models.NotificationSeverity.Info);

                this.RaisePropertyChanged(nameof(this.VertimagInverterConfiguration));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void ShowExport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.INVERTERSPARAMETERSEXPORT,
                data: this,
                trackCurrentView: true);
        }

        private void ShowImport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.INVERTERSPARAMETERSIMPORT,
                data: this,
                trackCurrentView: true);
        }

        private void ShowInverterParameters(Inverter inverterParametrers)
        {
            this.SelectedInverter = inverterParametrers;
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.PARAMETERSINVERTERDETAILS,
                data: this,
                trackCurrentView: true);
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            // exportable drives
            var drives = this.usbWatcher.Drives;
            try
            {
                this.exportableDrives = new ReadOnlyCollection<DriveInfo>(drives.Writable().ToList());
            }
            catch (Exception ex)
            {
                var exc = ex;
            }

            this.RaisePropertyChanged(nameof(this.AvailableDrives));

            // importable files
            var importables = drives.FindConfigurationFiles().ToList();
            this.importableFiles = new ReadOnlyCollection<FileInfo>(importables);
            this.RaisePropertyChanged(nameof(this.ImportableFiles));
            this.goToImport?.RaiseCanExecuteChanged();

            if (e.Attached.Any())
            {
                var count = importables.Count;
                var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                var message = string.Format(culture, Localized.Get("InstallationApp.MultipleConfigurationsDetected"), count);
                switch (count)
                {
                    case 1:
                        message = string.Format(culture, Localized.Get("InstallationApp.ConfigurationDetected"), string.Concat(importables[0].Name));
                        break;

                    case 0:
                        message = Localized.Get("InstallationApp.ExportableDeviceDetected");
                        break;
                }

                this.ShowNotification(message);
            }
            else
            {
                this.ClearNotifications();
            }
        }

        #endregion
    }
}
