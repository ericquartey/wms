using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.ViewModels;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class InvertersParametersExportViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly IUsbWatcherService usbWatcher;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private DelegateCommand exportCommand;

        private DelegateCommand exportVertimagCommand;

        private bool isBusy;

        private ISetVertimagInverterConfiguration parentConfiguration;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private DriveInfo drive = null;

        private bool hasNameConflict = false;

        private bool overwrite = false;

        #endregion

        #region Constructors

        public InvertersParametersExportViewModel(
            IMachineConfigurationWebService machineConfigurationWebService,
            IMachineDevicesWebService machineDevicesWebService,
            IMachineIdentityWebService identityService,
            IUsbWatcherService usbWatcher) : base(identityService)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public DriveInfo SelectedDrive
        {
            get => this.drive;
            set
            {
                var old = this.drive;
                if (this.SetProperty(ref this.drive, value))
                {
                    this.OnSelectedDriveChanged(old, value);
                    this.exportCommand?.RaiseCanExecuteChanged();
                    this.exportVertimagCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasFilenameConflict => this.hasNameConflict;

        public IEnumerable<DriveInfo> AvailableDrives => this.availableDrives;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ExportCommand =>
                   this.exportCommand
               ??
               (this.exportCommand = new DelegateCommand(
                async () => await this.ExportAsync(), this.CanExport));

        public ICommand ExportVertimagCommand =>
                  this.exportVertimagCommand
              ??
              (this.exportVertimagCommand = new DelegateCommand(
               async () => await this.ExportVertimagAsync(), this.CanExport));

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.exportCommand?.RaiseCanExecuteChanged();
                    this.exportVertimagCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public bool OverwriteTargetFile
        {
            get => this.overwrite;
            set => this.SetProperty(ref this.overwrite, value);
        }

        #endregion

        #region Methods

        private void OnSelectedDriveChanged(DriveInfo old, DriveInfo value)
        {
            var hasConflict = false;
            if (value != null)
            {
                try
                {
                    var fullPath = this.Filename(this.parentConfiguration.VertimagInverterConfiguration, this.SelectedDrive, false);
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

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            if (this.Data is ISetVertimagInverterConfiguration configuration)
            {
                this.parentConfiguration = configuration;
            }

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();

#if DEBUG
            this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
#else
            this.availableDrives = this.usbWatcher.Drives.Writable();
#endif
            this.RaisePropertyChanged(nameof(this.AvailableDrives));

            return base.OnAppearedAsync();
        }

        private bool CanExport()
        {
            return !this.IsBusy
                   &&
                   this.SelectedDrive != null
                   &&
                   this.parentConfiguration.VertimagInverterConfiguration != null;
        }

        private async Task ExportAsync()
        {
            var goback = false;
            try
            {
                if (this.HasFilenameConflict && this.OverwriteTargetFile)
                {
                    var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                    var messageBoxResult = dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmFileOverwrite"), Localized.Get("InstallationApp.FileIsAlreadyPresent"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                // fetch latest version

                var settings = new JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    ContractResolver = new Models.OrderedContractResolver(),
                    Converters = new Newtonsoft.Json.JsonConverter[]
                        {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                        },
                };

                var dbConfig = await this.machineDevicesWebService.GetInvertersAsync();

                var json = JsonConvert.SerializeObject(dbConfig, settings);

                var fullPath = this.Filename(this.parentConfiguration.VertimagInverterConfiguration, this.SelectedDrive, !this.OverwriteTargetFile);
                File.WriteAllText(fullPath, json);

                this.SelectedDrive = null;
                this.ShowNotification(Localized.Get("InstallationApp.ExportSuccessful"), Services.Models.NotificationSeverity.Success);
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

        private async Task ExportVertimagAsync()
        {
            var goback = false;
            try
            {
                if (this.HasFilenameConflict && this.OverwriteTargetFile)
                {
                    var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                    var messageBoxResult = dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmFileOverwrite"), Localized.Get("InstallationApp.FileIsAlreadyPresent"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                // fetch latest version

                var settings = new JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    ContractResolver = new Models.OrderedContractResolver(),
                    Converters = new Newtonsoft.Json.JsonConverter[]
                        {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                        },
                };

                var configuration = await this.machineConfigurationWebService.GetAsync();

                var json = JsonConvert.SerializeObject(configuration, settings);

                var fullPath = this.Filename(configuration, this.SelectedDrive, !this.OverwriteTargetFile);
                File.WriteAllText(fullPath, json);

                this.SelectedDrive = null;
                this.ShowNotification(Localized.Get("InstallationApp.ExportSuccessful"), Services.Models.NotificationSeverity.Success);
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

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.availableDrives = this.usbWatcher.Drives.Writable();
            this.RaisePropertyChanged(nameof(this.AvailableDrives));
            if (!this.availableDrives.Any())
            {
                this.ShowNotification(Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);

                // no need to await for this
                this.NavigationService.GoBackSafelyAsync();
            }
        }

        #endregion
    }
}
