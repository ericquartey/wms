using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParametersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IUsbWatcherService usbWatcher;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private readonly IMachineIdentityWebService identityService;

        private VertimagConfiguration configuration;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToExport;

        private DelegateCommand goToImport;

        private DelegateCommand changeIdCommand;

        private IEnumerable<FileInfo> importableFiles = Array.Empty<FileInfo>();

        private bool isBusy;

        private DelegateCommand saveCommand;

        private int newMachineId;

        private bool changeMachineIdVisible;

        #endregion

        #region Constructors

        public ParametersViewModel(
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usb)
            : base(Services.PresentationMode.Installer)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcher = usb;
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public VertimagConfiguration Configuration => this.configuration;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand GoToExport => this.goToExport
                    ??
            (this.goToExport = new DelegateCommand(
                this.ShowExport, this.CanShowExport));

        public ICommand GoToImport => this.goToImport
                    ??
            (this.goToImport = new DelegateCommand(
                this.ShowImport, this.CanShowImport));

        public ICommand ChangeIdCommand => this.changeIdCommand
                    ??
            (this.changeIdCommand = new DelegateCommand(
                async () => await this.ChangeIdAsync(), () => true));

        public IEnumerable<FileInfo> ImportableFiles => this.importableFiles;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.goToExport?.RaiseCanExecuteChanged();
                    this.goToImport?.RaiseCanExecuteChanged();
                    this.saveCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand SaveCommand =>
           this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
            async () => await this.SaveAsync(), this.CanSave));

        public int NewMachineId
        {
            get => this.newMachineId;
            set => this.SetProperty(ref this.newMachineId, value);
        }

        public bool ChangeMachineIdVisible
        {
            get => this.changeMachineIdVisible;
            set => this.SetProperty(ref this.changeMachineIdVisible, value);
        }

        #endregion

        #region Methods

        private async Task ChangeIdAsync()
        {
            try
            {
                await this.machineIdentityWebService.SetMachineIdAsync(this.newMachineId);

                this.ShowNotification(Resources.Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();

            this.ChangeMachineIdVisible = this.sessionService.UserAccessLevel == UserAccessLevel.Support ||
                                      this.sessionService.UserAccessLevel == UserAccessLevel.Admin; ;

            var model = await this.identityService.GetAsync();

            this.NewMachineId = model.Id;

#if DEBUG
            this.exportableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
            //  this.importableFiles = new ReadOnlyCollection<FileInfo>(DriveInfo.GetDrives().First().FindConfigurationFiles().ToList());
#endif

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                this.configuration = await this.machineConfigurationWebService.GetAsync();
                this.RaisePropertyChanged(nameof(this.Configuration));

                if (this.MachineService.Bay.Shutter?.Type == ShutterType.NotSpecified)
                {
                    this.ShowNotification(Resources.Localized.Get("InstallationApp.ShutterNotSpecifiedWarning"), Services.Models.NotificationSeverity.Warning);
                }
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

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args.PropertyName == nameof(this.IsMoving))
            {
                this.goToImport?.RaiseCanExecuteChanged();
            }
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private bool CanShowExport()
        {
            return this.AvailableDrives.Any();
        }

        private bool CanShowImport()
        {
            return this.ImportableFiles.Any();
        }

        private async Task SaveAsync()
        {
            try
            {
                // tabula rasa
                this.ClearNotifications();
                this.IsBusy = true;

                //await this.machineConfigurationWebService.SetAsync(this.configuration);
                await this.machineConfigurationWebService.UpdateAsync(this.configuration.Machine);

                this.ShowNotification(Resources.Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Success);

                //this.configuration = await this.machineConfigurationWebService.GetAsync();
                //this.RaisePropertyChanged(nameof(this.Configuration));

                await this.MachineService.OnUpdateServiceAsync();
                await this.OnDataRefreshAsync();

                this.IsBusy = false;
            }
            catch (Exception ex)
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
                Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                this.Configuration,
                trackCurrentView: true);
        }

        private void ShowImport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                // Utils.Modules.Installation.Parameters.PARAMETERSIMPORTSTEP1,
                nameof(ParametersImportViewModel),
                data: null,
                trackCurrentView: true);
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
            this.goToExport?.RaiseCanExecuteChanged();

            // importable files
            var importables = drives.FindConfigurationFiles().ToList();
            this.importableFiles = new ReadOnlyCollection<FileInfo>(importables);
            this.RaisePropertyChanged(nameof(this.ImportableFiles));
            this.goToImport?.RaiseCanExecuteChanged();

            if (e.Attached.Any())
            {
                var count = importables.Count;
                var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                var message = string.Format(culture, Resources.Localized.Get("InstallationApp.MultipleConfigurationsDetected"), count);
                switch (count)
                {
                    case 1:
                        message = string.Format(culture, Resources.Localized.Get("InstallationApp.ConfigurationDetected"), string.Concat(importables[0].Name));
                        break;

                    case 0:
                        message = Resources.Localized.Get("InstallationApp.ExportableDeviceDetected");
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
