using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParameterInverterViewModel : BaseParameterInverterViewModel, ISetVertimagInverterConfiguration
    {
        #region Fields

        private const string RESETDATA = "reset";

        private readonly List<FileInfo> importableFiles = new List<FileInfo>();

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly IBayManager bayManager;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToExport;

        private DelegateCommand goToImport;

        private string importFolderPath;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private IEnumerable<Inverter> inverters;

        private DelegateCommand readInvertersCommand;

        private DelegateCommand refreshCommand;

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
            IMachineMissionsWebService missionsWebService,
            IBayManager bayManager,
            IMachineDevicesWebService machineDevicesWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usbWatcher) : base(identityService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToExport =>
               this.goToExport
               ??
               (this.goToExport = new DelegateCommand(
                () => this.ShowExport()));

        public ICommand GoToImport =>
               this.goToImport
               ??
               (this.goToImport = new DelegateCommand(
                () => this.ShowImport()));

        public IEnumerable<FileInfo> ImportableFiles => this.importableFiles;

        public string ImportFolderPath
        {
            get => this.importFolderPath;
            set => this.SetProperty(ref this.importFolderPath, value);
        }

        public IEnumerable<Inverter> Inverters => this.inverters;

        public ICommand ReadInvertersCommand =>
                   this.readInvertersCommand
               ??
               (this.readInvertersCommand = new DelegateCommand(
                   async () => await this.ReadAllInvertersAsync(), this.CanRead));

        public ICommand RefreshCommand =>
               this.refreshCommand
               ??
               (this.refreshCommand = new DelegateCommand(
                async () => await this.RefreshAsync(), this.CanRefresh));

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
                   this.ShowInverterParameters, this.CanShowInverterParameter));

        public IEnumerable<Inverter> VertimagInverterConfiguration
        {
            get => this.vertimagInverterConfiguration;
            set => this.SetProperty(ref this.vertimagInverterConfiguration, value);
        }

        #endregion

        #region Methods

        public async void BackupVertimagInverterConfigurationParameters()
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

            var dbConfig = await this.machineDevicesWebService.GetInvertersAsync();

            var json = JsonConvert.SerializeObject(dbConfig, settings);

            var fullPath = this.Filename(this.VertimagInverterConfiguration, new DriveInfo(this.selectedFileConfiguration.DirectoryName), true);
            File.WriteAllText(fullPath, json);
        }

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            if (this.inverterReadingMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterReadingMessageData>>().Unsubscribe(this.inverterReadingMessageReceivedToken);
                this.inverterReadingMessageReceivedToken?.Dispose();
                this.inverterReadingMessageReceivedToken = null;
            }

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBusy = true;

            if (RESETDATA.Equals(this.Data))
            {
                this.SelectedFileConfiguration = null;
                this.VertimagInverterConfiguration = null;
                this.Data = null;
            }

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();

            this.SubscribeEvents();

            await base.OnAppearedAsync();

            this.IsBusy = false;

            var missions = await this.missionsWebService.GetAllAsync();

            if (missions.Any())
            {
                this.ShowNotification(Localized.Get("InstallationApp.MissionActive"), Services.Models.NotificationSeverity.Warning);
            }


        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                if (!(this.VertimagInverterConfiguration is null))
                {
                    this.inverters = this.VertimagInverterConfiguration;
                }
                else
                {
                    this.SelectedFileConfiguration = null;
                    this.inverters = await this.machineDevicesWebService.GetInvertersAsync();
                }

                this.inverters = this.inverters.OrderBy(s => s.Index);

                this.VertimagInverterConfiguration = this.inverters;
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
            this.readInvertersCommand?.RaiseCanExecuteChanged();
            this.goToImport?.RaiseCanExecuteChanged();
            this.goToExport?.RaiseCanExecuteChanged();
            this.refreshCommand?.RaiseCanExecuteChanged();
        }

        private bool CanRead()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private bool CanRefresh()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private bool CanShowImport()
        {
            return this.usbWatcher.Drives.Writable().Any();
        }

        private bool CanShowInverterParameter(Inverter inverter)
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private async Task OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.inverters = await this.machineDevicesWebService.GetInvertersAsync();
                    this.RaisePropertyChanged(nameof(this.inverters));
                    break;

                default:
                    break;
            }
        }

        private async Task ReadAllInvertersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.machineDevicesWebService.ReadAllInvertersAsync();
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

        private async Task RefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                var result = await this.machineDevicesWebService.GetInvertersAsync();
                this.inverters = result.OrderBy(s => s.Index);

                this.RaisePropertyChanged(nameof(this.Inverters));
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

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                this.BackupVertimagInverterConfigurationParameters();

                await this.machineDevicesWebService.ProgramAllInvertersAsync(this.vertimagInverterConfiguration);
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

        private void SubscribeEvents()
        {
            this.inverterReadingMessageReceivedToken = this.inverterReadingMessageReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                   .Subscribe(
                       async (m) => await this.OnInverterReadingMessageReceived(m),
                       ThreadOption.UIThread,
                       false);
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
            this.importableFiles.Clear();

            var dir = ConfigurationManager.AppSettings.GetInverterParametersPath();
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.json");
            }

            this.importableFiles.AddRange(FilterInverterConfigurationFile(this.usbWatcher.Drives.FindConfigurationFiles().ToList()));
            this.RaisePropertyChanged(nameof(this.ImportableFiles));
            this.goToImport?.RaiseCanExecuteChanged();
            this.goToExport?.RaiseCanExecuteChanged();

            if (e.Attached.Any())
            {
                var count = this.importableFiles.Count;
                var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                var message = string.Format(culture, Localized.Get("InstallationApp.MultipleConfigurationsDetected"), count);
                switch (count)
                {
                    case 1:
                        message = string.Format(culture, Localized.Get("InstallationApp.ConfigurationDetected"), string.Concat(this.importableFiles.ElementAt(0).Name));
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
