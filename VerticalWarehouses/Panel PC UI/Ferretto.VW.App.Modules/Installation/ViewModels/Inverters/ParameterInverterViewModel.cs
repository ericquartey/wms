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
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParameterInverterViewModel : BaseParameterInverterViewModel, ISetVertimagConfiguration
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly UsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToImport;

        private IEnumerable<FileInfo> importableFiles = Array.Empty<FileInfo>();

        private IEnumerable<InverterParametersData> invertersParameters;

        private string selectedFileConfigurationName;

        private Inverter selectedInverter;

        private DelegateCommand setInvertersParamertersCommand;

        private DelegateCommand<object> showInverterParamertersCommand;

        private VertimagConfiguration vertimagConfiguration;

        #endregion

        #region Constructors

        public ParameterInverterViewModel(IMachineDevicesWebService machineDevicesWebService, UsbWatcherService usb)
            : base()
        {
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.usbWatcher = usb;
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToImport => this.goToImport
                                    ??
                            (this.goToImport = new DelegateCommand(
                                this.ShowImport, this.CanShowImport));

        public IEnumerable<FileInfo> ImportableFiles => this.importableFiles;

        public IEnumerable<InverterParametersData> InvertersParameters => this.invertersParameters;

        public string SelectedFileConfigurationName
        {
            get => this.selectedFileConfigurationName;
            set => this.SetProperty(ref this.selectedFileConfigurationName, value);
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
       (this.showInverterParamertersCommand = new DelegateCommand<object>(this.ShowInverterParameters));

        public VertimagConfiguration VertimagConfiguration
        {
            get => this.vertimagConfiguration;
            set => this.SetProperty(ref this.vertimagConfiguration, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.Data = null;
            this.usbWatcher.DrivesChange -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Dispose();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            this.usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Start();

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
                    var invertersParameters = this.GetInvertersParameters(this.VertimagConfiguration);
                }
                else
                {
                    this.SelectedFileConfigurationName = "No file selected, current condfiguration loaded.";
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
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private bool CanShowImport()
        {
            return this.ImportableFiles.Any();
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
            }

            if (inverterParametersData.Count == 0)
            {
                throw new Exception("No inverters parameters found.");
            }

            return inverterParametersData;
        }

        private string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            return $"{type.ToString()} {ip}:{port}";
        }

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.machineDevicesWebService.ProgramAllInvertersAsync(new VertimagConfiguration());

                this.ShowNotification(InstallationApp.InvertersProgrammingStarted, Services.Models.NotificationSeverity.Info);

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
                data: null,
                trackCurrentView: true);
        }

        private void ShowInverterParameters(object paramerter)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.PARAMETERSINVERTERDETAILS,
                data: paramerter,
                trackCurrentView: true);
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            // exportable drives
            var drives = ((UsbWatcherService)sender).Drives;
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
                var message = string.Format(culture, InstallationApp.MultipleConfigurationsDetected, count);
                switch (count)
                {
                    case 1:
                        message = string.Format(culture, InstallationApp.ConfigurationDetected, string.Concat(importables[0].Name));
                        break;

                    case 0:
                        message = InstallationApp.ExportableDeviceDetected;
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
