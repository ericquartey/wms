using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class ParametersImportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService _machineConfigurationWebService;

        private readonly UsbWatcherService _usbWatcher;

        private bool cells = false;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private bool isBusy = false;

        private bool loadUnits = false;

        private bool parameters = false;

        private DelegateCommand saveCommand = null;

        private FileInfo selectedFile = null;

        #endregion

        #region Constructors

        public ParametersImportViewModel(IMachineConfigurationWebService machineConfigurationWebService, UsbWatcherService usb)
            : base(Services.PresentationMode.Installer)
        {
            this._machineConfigurationWebService = machineConfigurationWebService;
            this._usbWatcher = usb;
        }

        #endregion

        #region Properties

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public bool ImportCellPanels
        {
            get => this.cells;
            set => this.SetProperty(ref this.cells, value);
        }

        public bool ImportLoadUnits
        {
            get => this.loadUnits;
            set => this.SetProperty(ref this.loadUnits, value);
        }

        public bool ImportParameters
        {
            get => this.parameters;
            set => this.SetProperty(ref this.parameters, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.saveCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand RestoreCommand =>
                    this.saveCommand
                    ??
                    (this.saveCommand = new DelegateCommand(
                    this.Restore, this.CanRestore));

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set
            {
                if (this.SetProperty(ref this.selectedFile, value))
                {
                    this.saveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this._usbWatcher.DrivesChange -= this.UsbWatcher_DrivesChange;
            this._usbWatcher.Dispose();
            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this._usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
            this._usbWatcher.Start();

            return base.OnAppearedAsync();
        }

        private bool CanRestore()
        {
            return !this.IsBusy && this.SelectedFile != null;
        }

        private void Restore()
        {
            var file = this.selectedFile;
            var configuration = JsonConvert.DeserializeObject<VertimagConfiguration>(File.ReadAllText(file.FullName), new JsonConverter[] { new CommonUtils.Converters.IPAddressConverter() });
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            UsbWatcherService usb = (UsbWatcherService)sender;
            var configurationFiles = this.configurationFiles = usb.Drives.FindConfigurationFiles();
            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!configurationFiles.Any())
            {
                this.ShowNotification(Resources.InstallationApp.NoDevicesAvailableAnymore, Services.Models.NotificationSeverity.Warning);
                this.NavigationService.GoBack();
            }
        }

        #endregion
    }
}
