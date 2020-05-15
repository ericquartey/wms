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
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LogsExportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly UsbWatcherService usbWatcherService;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private DriveInfo drive = null;

        private DelegateCommand exportCommand;

        private bool hasNameConflict = false;

        private bool isBusy = false;

        private bool overwrite = false;

        #endregion

        //private bool includeAll = true;

        //private bool includeErrors = true;

        //private bool includeEventAggregator = true;

        //private bool includeMission = true;

        //private bool includeSqlCommand = true;

        #region Constructors

        public LogsExportViewModel(IMachineErrorsWebService machineErrorsWebService, UsbWatcherService usb)
                : base(PresentationMode.Operator)
        {
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

        //public bool IncludeAll
        //{
        //    get => this.includeAll;
        //    set => this.SetProperty(ref this.includeAll, value);
        //}

        //public bool IncludeErrors
        //{
        //    get => this.includeErrors;
        //    set => this.SetProperty(ref this.includeErrors, value);
        //}

        //public bool IncludeEventAggregator
        //{
        //    get => this.includeEventAggregator;
        //    set => this.SetProperty(ref this.includeEventAggregator, value);
        //}

        //public bool IncludeMission
        //{
        //    get => this.includeMission;
        //    set => this.SetProperty(ref this.includeMission, value);
        //}

        //public bool IncludeSqlCommand
        //{
        //    get => this.includeSqlCommand;
        //    set => this.SetProperty(ref this.includeSqlCommand, value);
        //}

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
            this.usbWatcherService.DrivesChange -= this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Dispose();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.usbWatcherService.DrivesChange += this.UsbWatcherService_DrivesChange;
            this.usbWatcherService.Start();

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
                   this.SelectedDrive != null;
        }

        private async Task ExportAsync()
        {
            var goback = false;
            try
            {
                this.ClearNotifications();

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

                // get actual executing location
                var filepath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                filepath += "\\" + "Logs";

                // Create path to export log
                var logFolder = "Logs_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "\\";
                var folderPath = System.IO.Path.Combine((this.SelectedDrive ?? throw new ArgumentNullException(nameof(this.SelectedDrive))).RootDirectory.FullName, logFolder);

                //
                System.IO.Directory.CreateDirectory(folderPath);

                var d = new DirectoryInfo(filepath);
                foreach (var file in d.GetFiles("*.log"))
                {
                    File.Copy(file.FullName, folderPath + file.Name, true);
                }

                //if (this.includeAll)            { File.Copy(filepath + "\\" + "All.log", folderPath); }
                //if (this.includeErrors)         { File.Copy(filepath + "\\" + "Errors.log", folderPath); }
                //if (this.includeEventAggregator){ File.Copy(filepath + "\\" + "Event-Aggregator.log", folderPath); }
                //if (this.includeMission)        { File.Copy(filepath + "\\" + "Mission.log", folderPath); }
                //if (this.includeSqlCommand)     { File.Copy(filepath + "\\" + "Sql-Commands.log", folderPath); }

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
                    this.NavigationService.GoBack();
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
                    // Create path
                    var logFolder = "Logs_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
                    var folderPath = System.IO.Path.Combine((this.SelectedDrive ?? throw new ArgumentNullException(nameof(this.SelectedDrive))).RootDirectory.FullName, logFolder);

                    hasConflict = Directory.Exists(folderPath);
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

            this.ClearNotifications();
        }

        private void UsbWatcherService_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            var drives = this.availableDrives = ((UsbWatcherService)sender).Drives ?? Array.Empty<DriveInfo>();
            this.RaisePropertyChanged(nameof(this.AvailableDrives));
            if (!drives.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
            }
            else
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.ExportableDeviceDetected"));
            }
        }

        #endregion
    }
}
