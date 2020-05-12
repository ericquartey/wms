using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json.Linq;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class InvertersParametersImportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService _machineConfigurationWebService;

        private readonly UsbWatcherService _usbWatcher;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private DelegateCommand importCommand = null;

        private bool isBusy = false;

        private ISetVertimagConfiguration parentConfiguration;

        private VertimagConfiguration selectedConfiguration = null;

        private FileInfo selectedFile = null;

        #endregion

        #region Constructors

        public InvertersParametersImportViewModel(IMachineConfigurationWebService machineConfigurationWebService, UsbWatcherService usb)
            : base(Services.PresentationMode.Installer)
        {
            this._machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this._usbWatcher = usb;
        }

        #endregion

        #region Properties

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ImportCommand =>
                    this.importCommand
                    ??
                    (this.importCommand = new DelegateCommand(
                    async () => await this.ImportAsync(), this.CanImport));

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.importCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public VertimagConfiguration SelectedConfiguration
        {
            get => this.selectedConfiguration;
            set => this.SetProperty(ref this.selectedConfiguration, value);
        }

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set
            {
                FileInfo old = this.selectedFile;
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
            this._usbWatcher.DrivesChange -= this.UsbWatcher_DrivesChange;

            this._usbWatcher.Dispose();
            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.SelectedFile = null;

            if (this.Data is ISetVertimagConfiguration)
            {
                this.parentConfiguration = (ISetVertimagConfiguration)this.Data;
            }

            this._usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
            this._usbWatcher.Start();
            this.FindConfigurationFiles();

            return base.OnAppearedAsync();
        }

        private bool CanImport()
        {
            return !this.IsBusy && !this.IsMoving && this.selectedConfiguration != null;
        }

        private void FindConfigurationFiles()
        {
            this.IsBusy = true;
            UsbWatcherService usb = this._usbWatcher;
            IEnumerable<FileInfo> configurationFiles = null;

            configurationFiles = this.configurationFiles = usb.Drives.FindConfigurationFiles();

            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!configurationFiles.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
                this.NavigationService.GoBackSafelyAsync();
            }
            this.IsBusy = false;
        }

        private async Task ImportAsync()
        {
            try
            {
                this.IsBusy = true;

                var source = await this._machineConfigurationWebService.GetAsync();

                // merge and save
                var result = source.ExtendWith(this.selectedConfiguration);
                var vertimagConfiguration = VertimagConfiguration.FromJson(result.ToString());

                this.parentConfiguration.SelectedFileConfigurationName = this.selectedFile.FullName;
                this.parentConfiguration.VertimagConfiguration = vertimagConfiguration;
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

        private void OnSelectedFileChanged(FileInfo _, FileInfo file)
        {
            VertimagConfiguration config = null;
            if (file != null)
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);
                    var source = new VertimagConfiguration
                    {
                        Machine = new Machine(),
                    }.ExtendWith(JObject.Parse(json));

                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<VertimagConfiguration>(source.ToString(),
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
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            this.FindConfigurationFiles();
        }

        #endregion
    }
}
