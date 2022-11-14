using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.ViewModels;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class InvertersParametersImportViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private readonly IUsbWatcherService usbWatcher;

        private List<FileInfo> configurationFiles = new List<FileInfo>();

        private DelegateCommand importCommand;

        private DelegateCommand importStructureCommand;

        private SubscriptionToken inverterParameterReceivedToken;

        private int inverterStructureCount;

        private bool isBusy;

        private ISetVertimagInverterConfiguration parentConfiguration;

        private IEnumerable<Inverter> selectedConfiguration;

        private FileInfo selectedFile;

        #endregion

        #region Constructors

        public InvertersParametersImportViewModel(
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService,
            IUsbWatcherService usbWatcher)
            : base(sessionService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ImportCommand =>
                   this.importCommand
               ??
               (this.importCommand = new DelegateCommand(
                () => this.ImportAsync(), this.CanImport));

        public ICommand ImportStructureCommand =>
                   this.importStructureCommand
               ??
               (this.importStructureCommand = new DelegateCommand(
                () =>
                {
                    this.IsBusy = true;
                    this.inverterStructureCount = this.selectedConfiguration.Count();
                    this.parentConfiguration.SelectedFileConfiguration = this.selectedFile;
                    this.parentConfiguration.VertimagInverterConfiguration = this.selectedConfiguration;
                    this.ShowNotification(Resources.Localized.Get("InstallationApp.CommandSent"), Services.Models.NotificationSeverity.Info);
                    this.machineDevicesWebService.ImportInvertersStructureAsync(this.selectedConfiguration);
                }, this.CanImport));

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.importCommand?.RaiseCanExecuteChanged();
                    this.importStructureCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public IEnumerable<Inverter> SelectedConfiguration
        {
            get => this.selectedConfiguration;
            set => this.SetProperty(ref this.selectedConfiguration, value);
        }

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set
            {
                var old = this.selectedFile;
                if (this.SetProperty(ref this.selectedFile, value))
                {
                    this.OnSelectedFileChanged(old, value);
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            if (this.inverterParameterReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterParametersMessageData>>().Unsubscribe(this.inverterParameterReceivedToken);
                this.inverterParameterReceivedToken?.Dispose();
                this.inverterParameterReceivedToken = null;
            }

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.SelectedFile = null;

            if (this.Data is ISetVertimagInverterConfiguration configuration)
            {
                this.parentConfiguration = configuration;
            }

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();
            this.FindConfigurationFiles();

            this.SubscribeEvents();

            return base.OnAppearedAsync();
        }

        private bool CanImport()
        {
            return !this.IsBusy &&
                !this.IsMoving &&
                this.selectedConfiguration != null &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private void FindConfigurationFiles()
        {
            this.IsBusy = true;

            this.configurationFiles.Clear();

            var dir = ConfigurationManager.AppSettings.GetInverterParametersPath();
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.json");
                this.configurationFiles = FilterInverterConfigurationFile(files).ToList();
            }

            this.configurationFiles.AddRange(FilterInverterConfigurationFile(this.usbWatcher.Drives.FindConfigurationFiles().ToList()));

            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!this.configurationFiles.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
            }

            this.IsBusy = false;
        }

        private void ImportAsync()
        {
            try
            {
                this.IsBusy = true;

                this.parentConfiguration.SelectedFileConfiguration = this.selectedFile;
                this.parentConfiguration.VertimagInverterConfiguration = this.selectedConfiguration;
                this.ShowNotification(Resources.Localized.Get("InstallationApp.ImportSuccessful"), Services.Models.NotificationSeverity.Success);
                this.NavigationService.GoBack();
            }
            catch (Exception exc)
            {
                this.ShowNotification(exc);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void OnInverterParameterReceived(NotificationMessageUI<InverterParametersMessageData> message)
        {
            if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationUpdateData)//import structure command
            {
                this.inverterStructureCount--;
                if (this.inverterStructureCount == 0)
                {
                    this.IsBusy = false;
                }
            }
        }

        private void OnSelectedFileChanged(FileInfo _, FileInfo file)
        {
            IEnumerable<Inverter> config = null;
            this.ClearNotifications();
            if (file != null)
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);

                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Inverter>>(json.ToString(),
                        new Newtonsoft.Json.JsonConverter[]
                        {
                            new CommonUtils.Converters.IPAddressConverter(),
                            new Newtonsoft.Json.Converters.StringEnumConverter(),
                        });
                }
                catch (Exception exc)
                {
                    this.ShowNotification(exc);
                }
            }
            this.selectedConfiguration = config;
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));

            this.importCommand?.RaiseCanExecuteChanged();
            this.importStructureCommand?.RaiseCanExecuteChanged();
        }

        private void SubscribeEvents()
        {
            this.inverterParameterReceivedToken = this.inverterParameterReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterParametersMessageData>>()
                   .Subscribe(
                       (m) => this.OnInverterParameterReceived(m),
                       ThreadOption.UIThread,
                       false);
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.FindConfigurationFiles();
        }

        #endregion
    }
}
