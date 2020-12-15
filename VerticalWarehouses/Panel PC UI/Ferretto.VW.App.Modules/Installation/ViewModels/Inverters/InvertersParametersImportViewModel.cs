using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
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
    public class InvertersParametersImportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private DelegateCommand importCommand;

        private bool isBusy;

        private ISetVertimagConfiguration parentConfiguration;

        private VertimagConfiguration selectedConfiguration;

        private FileInfo selectedFile;

        #endregion

        #region Constructors

        public InvertersParametersImportViewModel(
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usbWatcher)
            : base(PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
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

            if (this.Data is ISetVertimagConfiguration configuration)
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
            return !this.IsBusy && !this.IsMoving && this.selectedConfiguration != null;
        }

        private void FindConfigurationFiles()
        {
            this.IsBusy = true;
            this.configurationFiles = this.usbWatcher.Drives.FindConfigurationFiles();

            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!this.configurationFiles.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
                //this.NavigationService.GoBackSafelyAsync();
            }

            this.IsBusy = false;
        }

        private async Task ImportAsync()
        {
            try
            {
                this.IsBusy = true;

                var source = await this.machineConfigurationWebService.GetAsync();

                // merge and save
                var result = source.ExtendWith(this.selectedConfiguration);
                var vertimagConfiguration = VertimagConfiguration.FromJson(result.ToString());

                this.parentConfiguration.SelectedFileConfiguration = this.selectedFile;
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
            this.ClearNotifications();
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

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.FindConfigurationFiles();
        }

        #endregion
    }
}
