using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Newtonsoft.Json.Linq;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class ParametersImportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService _machineConfigurationWebService;

        private readonly UsbWatcherService _usbWatcher;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private VertimagConfiguration importingConfiguration = null;

        private bool isBusy = false;

        private DelegateCommand saveCommand = null;

        private VertimagConfiguration selectedConfiguration = null;

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

        /// <summary>
        /// Gets or sets the configuration ready to be imported, if any.
        /// </summary>
        public VertimagConfiguration ImportingConfiguration
        {
            get => this.importingConfiguration;
            set => this.SetProperty(ref this.importingConfiguration, value);
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
                    async () => await this.RestoreAsync(), this.CanRestore));

        /// <summary>
        /// Gets or sets the configuration stored in the current <see cref="SelectedFile"/>, if any.
        /// </summary>
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
            // reset selected file to null, just in case
            this.SelectedFile = null;

            this._usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
            this._usbWatcher.Start();

            return base.OnAppearedAsync();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args?.PropertyName == nameof(this.IsMoving) || args?.PropertyName == nameof(this.ImportingConfiguration))
            {
                this.saveCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanRestore()
        {
            return !this.IsBusy && !this.IsMoving && this.ImportingConfiguration != null;
        }

        private void OnSelectedFileChanged(FileInfo _, FileInfo file)
        {
            VertimagConfiguration config = null;
            if (file != null)
            {
                try
                {
                    config = VertimagConfiguration.FromJson(File.ReadAllText(file.FullName));
                }
                catch (Exception exc)
                {
                    this.ShowNotification(exc);
                }
            }
            this.selectedConfiguration = config;
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private async Task RestoreAsync()
        {
            try
            {
                // fetch the very last version
                var source = await this._machineConfigurationWebService.GetAsync();

                // merge and save
                JObject result = JObject.Parse(source.ToJson());
                result.Merge(JObject.Parse(this.ImportingConfiguration.ToJson()), new JsonMergeSettings
                {
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    PropertyNameComparison = StringComparison.Ordinal,
                });
                var target = VertimagConfiguration.FromJson(result.ToString());

                await this._machineConfigurationWebService.SetAsync(target);
            }
            catch (Exception exc)
            {
                this.ShowNotification(exc);
            }
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            this.IsBusy = true;
            UsbWatcherService usb = (UsbWatcherService)sender;
            var configurationFiles = this.configurationFiles = usb.Drives.FindConfigurationFiles();
            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!configurationFiles.Any())
            {
                this.ShowNotification(Resources.InstallationApp.NoDevicesAvailableAnymore, Services.Models.NotificationSeverity.Warning);
                this.NavigationService.GoBackSafelyAsync(); //.ConfigureAwait(false).GetAwaiter().GetResult();
            }
            this.IsBusy = false;
        }

        #endregion
    }
}
