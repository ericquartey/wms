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
    internal sealed class ParameterInverterViewModel : BaseParameterInverterViewModel, ISetVertimagConfiguration
    {
        #region Fields

        private const string RESETDATA = "reset";

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToImport;

        private IEnumerable<FileInfo> importableFiles = Array.Empty<FileInfo>();

        private string importFolderPath;

        private IEnumerable<InverterParametersData> invertersParameters;

        private FileInfo selectedFileConfiguration;

        private InverterParametersData selectedInverter;

        private DelegateCommand setInvertersParamertersCommand;

        private DelegateCommand<InverterParametersData> showInverterParamertersCommand;

        private VertimagConfiguration vertimagConfiguration;

        #endregion

        #region Constructors

        public ParameterInverterViewModel(
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usbWatcher)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

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

        public IEnumerable<InverterParametersData> InvertersParameters => this.invertersParameters;

        public FileInfo SelectedFileConfiguration
        {
            get => this.selectedFileConfiguration;
            set => this.SetProperty(ref this.selectedFileConfiguration, value);
        }

        public InverterParametersData SelectedInverter
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
               (this.showInverterParamertersCommand = new DelegateCommand<InverterParametersData>(
                   this.ShowInverterParameters));

        public VertimagConfiguration VertimagConfiguration
        {
            get => this.vertimagConfiguration;
            set => this.SetProperty(ref this.vertimagConfiguration, value);
        }

        #endregion

        #region Methods

        public async Task BackupVertimagConfigurationParameters()
        {
            if (this.selectedFileConfiguration is null)
            {
                return;
            }

            var currentConfiguration = await this.machineConfigurationWebService.GetAsync();
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

            var json = JsonConvert.SerializeObject(currentConfiguration, settings);
            var fullPath = currentConfiguration.Filename(new DriveInfo(this.selectedFileConfiguration.DirectoryName), true);

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
                this.VertimagConfiguration = null;
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

                if (!(this.VertimagConfiguration is null))
                {
                    this.invertersParameters = this.GetInvertersParameters(this.VertimagConfiguration);
                }
                else
                {
                    this.SelectedFileConfiguration = null;
                    this.invertersParameters = await this.machineDevicesWebService.GetParametersAsync();
                }

                this.RaisePropertyChanged(nameof(this.InvertersParameters));
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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.setInvertersParamertersCommand?.RaiseCanExecuteChanged();
            this.showInverterParamertersCommand?.RaiseCanExecuteChanged();
            this.goToImport?.RaiseCanExecuteChanged();
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

        private IEnumerable<InverterParametersData> GetInvertersParameters(VertimagConfiguration vertimagConfiguration)
        {
            var inverterParametersData = new List<InverterParametersData>();

            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter?.Parameters is null))
                {
                    inverterParametersData.Add(new InverterParametersData() { InverterIndex = (byte)axe.Inverter.Index, Description = this.GetShortInverterDescription(axe.Inverter.Type, axe.Inverter.IpAddress, axe.Inverter.TcpPort), Parameters = axe.Inverter.Parameters });
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter?.Parameters is null))
                {
                    inverterParametersData.Add(new InverterParametersData() { InverterIndex = (byte)bay.Inverter.Index, Description = this.GetShortInverterDescription(bay.Inverter.Type, bay.Inverter.IpAddress, bay.Inverter.TcpPort), Parameters = bay.Inverter.Parameters });
                }

                if (!(bay.Shutter?.Inverter?.Parameters is null))
                {
                    inverterParametersData.Add(new InverterParametersData() { InverterIndex = (byte)bay.Shutter.Inverter.Index, Description = this.GetShortInverterDescription(bay.Shutter.Inverter.Type, bay.Shutter.Inverter.IpAddress, bay.Shutter.Inverter.TcpPort), Parameters = bay.Shutter.Inverter.Parameters });
                }
            }

            if (inverterParametersData.Count == 0)
            {
                throw new ArgumentException("No inverters parameters found.");
            }

            return inverterParametersData;
        }

        private string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            var ipPort = (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";
            return $"{type.ToString()} {ipPort}";
        }

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.BackupVertimagConfigurationParameters();

                await this.machineDevicesWebService.ProgramAllInvertersAsync(this.vertimagConfiguration);

                this.ShowNotification(Localized.Get("InstallationApp.InvertersProgrammingStarted"), Services.Models.NotificationSeverity.Info);

                this.RaisePropertyChanged(nameof(this.VertimagConfiguration));
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

        private void ShowImport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.INVERTERSPARAMETERSIMPORT,
                data: this,
                trackCurrentView: true);
        }

        private void ShowInverterParameters(InverterParametersData inverterParametrers)
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
