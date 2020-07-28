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
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersExportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly IUsbWatcherService usbWatcherService;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private object configuration = null;

        private DriveInfo drive = null;

        private DelegateCommand exportCommand;

        private bool hasNameConflict = false;

        private bool isBusy = false;

        private bool overwrite = false;

        #endregion

        #region Constructors

        public ParametersExportViewModel(IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usb,
            IMachineServicingWebService machineServicingWebService)
            : base(PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcherService = usb ?? throw new ArgumentNullException(nameof(usb));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
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
                var old = this.drive;
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
            this.usbWatcherService.DrivesChanged -= this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Disable();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.usbWatcherService.DrivesChanged += this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Enable();

#if DEBUG
            this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
#else
            this.availableDrives = this.usbWatcherService.Drives.Writable();
#endif
            this.RaisePropertyChanged(nameof(this.AvailableDrives));

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
                var output = this.ExportingConfiguration as VertimagConfiguration;
                var configuration = this.Data as VertimagConfiguration;

                output = await this.UpdateOutputAsync(output);

                var settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new Models.OrderedContractResolver(),
                    Converters = new JsonConverter[]
                    {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                    },
                };
                var json = JsonConvert.SerializeObject(output, settings);

                var fullPath = configuration.Filename(this.SelectedDrive, !this.OverwriteTargetFile);
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

        private void OnSelectedDriveChanged(DriveInfo old, DriveInfo value)
        {
            var hasConflict = false;
            if (value != null)
            {
                try
                {
                    var vertimag = this.Data as VertimagConfiguration;

                    var fullPath = vertimag.Filename(value, false);// this.GetFullPath(value, vertimag.Machine.SerialNumber);
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

        private async Task<VertimagConfiguration> UpdateOutputAsync(VertimagConfiguration output)
        {
            var outsafe = output;
            try
            {
                //fix null Accessories
                if (Login.ScaffolderUserAccesLevel.UseAccessories)
                {
                    for (int i = 0; i < output.Machine.Bays.Count(); i++)
                    {
                        if (output.Machine.Bays.ElementAtOrDefault(i).Accessories == null)
                        {
                            var config = this.MachineService.Bays.Where(s => s.Id == output.Machine.Bays.ElementAtOrDefault(i).Id).FirstOrDefault();
                            output.Machine.Bays.ElementAtOrDefault(i).Accessories = config.Accessories;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < output.Machine.Bays.Count(); i++)
                    {
                        if (output.Machine.Bays.ElementAtOrDefault(i).Accessories != null)
                        {
                            output.Machine.Bays.ElementAtOrDefault(i).Accessories = null;
                        }
                    }
                }

                //fix null Instructions
                var service = await this.machineServicingWebService.GetAllAsync();

                for (int i = 0; i < output.ServicingInfo.Count(); i++)
                {
                    if (output.ServicingInfo.ElementAtOrDefault(i).Instructions == null)
                    {
                        output.ServicingInfo.ElementAtOrDefault(i).Instructions = service.Where(w => w.Id == output.ServicingInfo.ElementAtOrDefault(i).Id).LastOrDefault().Instructions;
                    }
                }

                return output;
            }
            catch (Exception)
            {
                return outsafe;
            }
        }

        private void UsbWatcherService_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.availableDrives = this.usbWatcherService.Drives.Writable();
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
