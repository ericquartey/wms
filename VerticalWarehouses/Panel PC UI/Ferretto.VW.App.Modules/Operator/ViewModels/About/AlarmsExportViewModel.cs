using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class AlarmsExportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly DelegateCommand exportCommand;

        private readonly string separator = ";";

        private readonly IUsbWatcherService usbWatcherService;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private DriveInfo drive = null;

        private bool hasNameConflict;

        private bool includeBayNumber = true;

        private bool includeCode = true;

        private bool includeDescription = true;

        private bool includeDetailCode = true;

        private bool includeID = true;

        private bool includeInverterIndex = true;

        private bool includeOccurenceDate = true;

        private bool includeResolutionDate = true;

        private bool isBusy;

        private IEnumerable<MachineError> machineErrors;

        private bool overwrite;

        #endregion

        #region Constructors

        public AlarmsExportViewModel(IUsbWatcherService usbWatcherService)
            : base(PresentationMode.Operator)
        {
            this.usbWatcherService = usbWatcherService ?? throw new ArgumentNullException(nameof(usbWatcherService));
            this.exportCommand = new DelegateCommand(this.Export, this.CanExport);
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives
        {
            get => this.availableDrives;
            set => this.SetProperty(ref this.availableDrives, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ExportCommand => this.exportCommand;

        public bool HasFilenameConflict => this.hasNameConflict;

        public bool IncludeBayNumber
        {
            get => this.includeBayNumber;
            set => this.SetProperty(ref this.includeBayNumber, value);
        }

        public bool IncludeCode
        {
            get => this.includeCode;
            set => this.SetProperty(ref this.includeCode, value);
        }

        public bool IncludeDescription
        {
            get => this.includeDescription;
            set => this.SetProperty(ref this.includeDescription, value);
        }

        public bool IncludeDetailCode
        {
            get => this.includeDetailCode;
            set => this.SetProperty(ref this.includeDetailCode, value);
        }

        public bool IncludeID
        {
            get => this.includeID;
            set => this.SetProperty(ref this.includeID, value);
        }

        public bool IncludeInverterIndex
        {
            get => this.includeInverterIndex;
            set => this.SetProperty(ref this.includeInverterIndex, value);
        }

        public bool IncludeOccurenceDate
        {
            get => this.includeOccurenceDate;
            set => this.SetProperty(ref this.includeOccurenceDate, value);
        }

        public bool IncludeResolutionDate
        {
            get => this.includeResolutionDate;
            set => this.SetProperty(ref this.includeResolutionDate, value);
        }

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

        public IEnumerable<MachineError> MachineErrors
        {
            get => this.machineErrors;
            private set
            {
                if (this.SetProperty(ref this.machineErrors, value))
                {
                    this.exportCommand?.RaiseCanExecuteChanged();
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
            this.usbWatcherService.DrivesChanged -= this.UsbWatcherService_DrivesChanged;
            this.usbWatcherService.Disable();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.usbWatcherService.DrivesChanged += this.UsbWatcherService_DrivesChanged;
            this.usbWatcherService.Enable();

#if DEBUG
            // this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
            // this.RaisePropertyChanged(nameof(this.AvailableDrives));
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
                   this.SelectedDrive != null;
        }

        private void Export()
        {
            if (this.SelectedDrive is null)
            {
                this.ShowNotification(Localized.Get("InstallationApp.NoDriveSelected"));
            }

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

                // Data has been passed to this ViewModel from previous view model (AlarmViewModel).
                var lst = this.Data as List<MachineError>;
                this.MachineErrors = lst.OrderBy(o => o.OccurrenceDate).ToList();

                // Create filename and full path
                var filename = "Alarms_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv";
                var fullPath = Path.Combine(this.SelectedDrive.RootDirectory.FullName, filename);

                // create intestation
                // HACK: very bad performance. Consider replacing with StringBuilder or, even better, writing directly to file
                var intestation = string.Empty;
                intestation += this.IncludeID ? Localized.Get("OperatorApp.Id") + this.separator : string.Empty;
                intestation += this.IncludeBayNumber ? Localized.Get("OperatorApp.BayNumber") + this.separator : string.Empty;
                intestation += this.IncludeCode ? Localized.Get("OperatorApp.Code") + this.separator : string.Empty;
                intestation += this.IncludeDescription ? Localized.Get("OperatorApp.Description") + this.separator : string.Empty;
                intestation += this.IncludeDetailCode ? Localized.Get("OperatorApp.DetailCode") + this.separator : string.Empty;
                intestation += this.IncludeInverterIndex ? Localized.Get("OperatorApp.InverterIndex") + this.separator : string.Empty;
                intestation += this.IncludeOccurenceDate ? Localized.Get("OperatorApp.OccurrenceDate") + this.separator : string.Empty;
                intestation += this.IncludeResolutionDate ? Localized.Get("OperatorApp.ResolutionDate") + this.separator : string.Empty;

                File.WriteAllText(fullPath, intestation + "\n");

                File.AppendAllLines(fullPath, this.MachineErrors.Select(e =>
                {
                    var line = string.Empty;
                    // HACK: very bad performance. Consider replacing with StringBuilder or, even better, writing directly to file

                    line += this.IncludeID ? e.Id + this.separator : string.Empty;
                    line += this.IncludeBayNumber ? e.BayNumber + this.separator : string.Empty;
                    line += this.IncludeCode ? e.Code + this.separator : string.Empty;
                    line += this.IncludeDescription ? e.Description + this.separator : string.Empty;
                    line += this.IncludeDetailCode ? e.DetailCode + this.separator : string.Empty;
                    line += this.IncludeInverterIndex ? e.InverterIndex + this.separator : string.Empty;
                    line += this.IncludeOccurenceDate ? e.OccurrenceDate + this.separator : string.Empty;
                    line += this.IncludeResolutionDate ? e.ResolutionDate + this.separator : string.Empty;

                    return line;
                }));

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
                    // Create filename and full path
                    var filename = "Alarms_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv";
                    var fullPath = Path.Combine((this.SelectedDrive ?? throw new ArgumentNullException(nameof(this.SelectedDrive))).RootDirectory.FullName, filename);

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

            this.ClearNotifications();
        }

        private void UsbWatcherService_DrivesChanged(object sender, DrivesChangedEventArgs e)
        {
            this.AvailableDrives = this.usbWatcherService.Drives;

            if (this.AvailableDrives.Any())
            {
                this.ShowNotification(Localized.Get("InstallationApp.ExportableDeviceDetected"));
            }
            else
            {
                this.ShowNotification(Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
            }
        }

        #endregion
    }
}
