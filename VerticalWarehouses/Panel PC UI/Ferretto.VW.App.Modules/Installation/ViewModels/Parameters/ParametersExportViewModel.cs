using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersExportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly UsbWatcherService usbWatcherService;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private object configuration = null;

        private DriveInfo drive = null;

        private DelegateCommand exportCommand;

        private bool hasNameConflict = false;

        private bool isBusy = false;

        private bool overwrite = false;

        #endregion

        #region Constructors

        public ParametersExportViewModel(IMachineConfigurationWebService machineConfigurationWebService, UsbWatcherService usb)
                : base(PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcherService = usb ?? throw new ArgumentNullException(nameof(usb));
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.availableDrives;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ExportCommand =>
            this.exportCommand ??
            (this.exportCommand = new DelegateCommand(
            async () => await this.ExportAsync(), this.CanExport));

        public object ExportingConfiguration
        {
            get => this.configuration;
            set
            {
                if (this.SetProperty(ref this.configuration, value))
                {
                    this.exportCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasFilenameConflict => this.hasNameConflict;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.exportCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public bool OverwriteTargetFile
        {
            get => this.overwrite;
            set => this.SetProperty(ref this.overwrite, value);
        }

        public DriveInfo SelectedDrive
        {
            get => this.drive;
            set
            {
                DriveInfo old = this.drive;
                if (this.SetProperty(ref this.drive, value))
                {
                    this.OnSelectedDriveChanged(old, value);
                    this.exportCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcherService.DrivesChange -= this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Dispose();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.usbWatcherService.DrivesChange += this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Start();

#if DEBUG
            this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
            this.RaisePropertyChanged(nameof(this.AvailableDrives));
#endif

            this.RaisePropertyChanged(nameof(this.Data));

            return base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.exportCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanExport()
        {
            return !this.IsBusy
                   &&
                   this.SelectedDrive != null
                   &&
                   this.ExportingConfiguration != null
                   ;
        }

        private async Task ExportAsync()
        {
            bool goback = false;
            try
            {
                if (this.HasFilenameConflict && this.OverwriteTargetFile)
                {
                    var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                    var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmFileOverwrite, InstallationApp.FileIsAlreadyPresent, DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                // fetch latest version
                var output = this.ExportingConfiguration;
                var configuration = this.Data as VertimagConfiguration;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(output,
                    new Newtonsoft.Json.JsonConverter[]
                    {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                    });

                string fullPath = configuration.Filename(this.SelectedDrive, !this.OverwriteTargetFile);
                File.WriteAllText(fullPath, json);

                this.SelectedDrive = null;
                this.ShowNotification(InstallationApp.ExportSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;

                if (goback)
                {
                    await this.NavigationService.GoBackSafelyAsync();
                }
            }
        }

        private void OnSelectedDriveChanged(DriveInfo old, DriveInfo value)
        {
            bool hasConflict = false;
            if (value != null)
            {
                try
                {
                    var vertimag = this.Data as VertimagConfiguration;
                    string fullPath = vertimag.Filename(value, false);// this.GetFullPath(value, vertimag.Machine.SerialNumber);
                    hasConflict = File.Exists(fullPath);
                }
                catch (Exception exc)
                {
                    this.ShowNotification(exc);
                }
            }
            this.hasNameConflict = this.overwrite = hasConflict;
            this.RaisePropertyChanged(nameof(this.HasFilenameConflict));
            this.RaisePropertyChanged(nameof(this.OverwriteTargetFile));
            this.exportCommand?.RaiseCanExecuteChanged();
        }

        private void UsbWatcherService_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            var drives = this.availableDrives = ((UsbWatcherService)sender).Drives.Writable() ?? Array.Empty<DriveInfo>();
            this.RaisePropertyChanged(nameof(this.AvailableDrives));
            if (!drives.Any())
            {
                this.ShowNotification(Resources.InstallationApp.NoDevicesAvailableAnymore, Services.Models.NotificationSeverity.Warning);

                // no need to await for this
                this.NavigationService.GoBackSafelyAsync();
            }
        }

        #endregion
    }
}
