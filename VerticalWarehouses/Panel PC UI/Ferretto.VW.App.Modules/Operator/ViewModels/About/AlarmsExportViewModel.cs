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
    public class AlarmsExportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private List<MachineError> machineErrors;

        private readonly UsbWatcherService usbWatcherService;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private DriveInfo drive = null;

        private DelegateCommand exportCommand;

        private bool hasNameConflict = false;

        private bool isBusy = false;

        private bool overwrite = false;

        private readonly string separator = ";";

        private bool includeID = true;

        private bool includeBayNumber = true;

        private bool includeCode = true;

        private bool includeDescription = true;

        private bool includeDetailCode = true;

        private bool includeInverterIndex = true;

        private bool includeOccurenceDate = true;

        private bool includeResolutionDate = true;

        #endregion

        #region Constructors

        public AlarmsExportViewModel(IMachineErrorsWebService machineErrorsWebService, UsbWatcherService usb)
                : base(PresentationMode.Operator)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
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

        public List<MachineError> MachineErrors
        {
            get => this.machineErrors;
            set
            {
                if (this.SetProperty(ref this.machineErrors, value))
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

        public bool IncludeID
        {
            get => this.includeID;
            set => this.SetProperty(ref this.includeID, value);
        }

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
            //this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
            //this.RaisePropertyChanged(nameof(this.AvailableDrives));
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

        private async Task ExportAsync()
        {
            bool goback = false;
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
                string filename = "Alarms_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv";
                string fullPath = System.IO.Path.Combine((this.SelectedDrive ?? throw new ArgumentNullException(nameof(this.SelectedDrive))).RootDirectory.FullName, filename);

                //create intestation
                string intestation = "";
                intestation += this.IncludeID ? OperatorApp.Id + this.separator : string.Empty;
                intestation += this.IncludeBayNumber ? OperatorApp.BayNumber + this.separator : string.Empty;
                intestation += this.IncludeCode ? OperatorApp.Code + this.separator : string.Empty;
                intestation += this.IncludeDescription ? OperatorApp.Description + this.separator : string.Empty;
                intestation += this.IncludeDetailCode ? OperatorApp.DetailCode + this.separator : string.Empty;
                intestation += this.IncludeInverterIndex ? OperatorApp.InverterIndex + this.separator : string.Empty;
                intestation += this.IncludeOccurenceDate ? OperatorApp.OccurrenceDate + this.separator : string.Empty;
                intestation += this.IncludeResolutionDate ? OperatorApp.ResolutionDate + this.separator : string.Empty;

                File.WriteAllText(fullPath, intestation + "\n");

                File.AppendAllLines(fullPath, this.MachineErrors.Select(e =>
                   {
                       string line = "";

                       line += this.IncludeID ? e.Id + this.separator : string.Empty;
                       line += this.IncludeBayNumber ? e.BayNumber + this.separator : string.Empty;
                       line += this.IncludeCode ? e.Code + this.separator : string.Empty;
                       line += this.IncludeDescription ? e.Description + this.separator : string.Empty;
                       line += this.IncludeDetailCode ? e.DetailCode + this.separator : string.Empty;
                       line += this.IncludeInverterIndex ? e.InverterIndex + this.separator : string.Empty;
                       line += this.IncludeOccurenceDate ? e.OccurrenceDate + this.separator : string.Empty;
                       line += this.IncludeResolutionDate ? e.ResolutionDate + this.separator : string.Empty;

                       return line;
                   }

                 ));

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
            bool hasConflict = false;
            if (value != null)
            {
                try
                {
                    // Create filename and full path
                    string filename = "Alarms_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv";
                    string fullPath = System.IO.Path.Combine((this.SelectedDrive ?? throw new ArgumentNullException(nameof(this.SelectedDrive))).RootDirectory.FullName, filename);

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
