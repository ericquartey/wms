using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.ViewModels;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json.Linq;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class InvertersParametersImportViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private DelegateCommand importCommand;

        private DelegateCommand importStructureCommand;

        private bool isBusy;

        private ISetVertimagInverterConfiguration parentConfiguration;

        private IEnumerable<Inverter> selectedConfiguration;

        private FileInfo selectedFile;

        #endregion

        #region Constructors

        public InvertersParametersImportViewModel(
            IMachineIdentityWebService identityService,
            IMachineDevicesWebService machineDevicesWebService,
            IUsbWatcherService usbWatcher)
            : base(identityService)
        {
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
                async () => await this.ImportStructureAsync(), this.CanImport));

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

            return base.OnAppearedAsync();
        }

        private bool CanImport()
        {
            return !this.IsBusy &&
                !this.IsMoving &&
                this.selectedConfiguration != null;
        }

        private void FindConfigurationFiles()
        {
            this.IsBusy = true;
            //this.configurationFiles = this.usbWatcher.Drives.FindConfigurationFiles();
            this.configurationFiles = FilterInverterConfigurationFile(this.usbWatcher.Drives.FindConfigurationFiles());

            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!this.configurationFiles.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
                //this.NavigationService.GoBackSafelyAsync();
            }

            this.IsBusy = false;
        }

        private void ImportAsync()
        {
            try
            {
                this.IsBusy = true;

                //var source = await this.machineConfigurationWebService.GetAsync();

                //// merge and save
                //var result = source.ExtendWith(this.selectedConfiguration);
                //var vertimagConfiguration = VertimagInverterConfiguration.FromJson(this.selectedConfiguration.ToString());

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

        private async Task ImportStructureAsync()
        {
            this.IsBusy = true;

            await this.machineDevicesWebService.ImportInvertersStructureAsync(this.selectedConfiguration);

            this.IsBusy = false;
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

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.FindConfigurationFiles();
        }

        #endregion
    }
}
