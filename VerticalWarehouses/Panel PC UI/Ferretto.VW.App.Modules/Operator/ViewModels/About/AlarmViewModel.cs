using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class AlarmViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly UsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToExport;

        private List<MachineError> machineErrors;

        #endregion

        #region Constructors

        public AlarmViewModel(
            IMachineErrorsWebService machineErrorsWebService, UsbWatcherService usbWatcher)
                    : base()
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.usbWatcher = usbWatcher ?? throw new ArgumentNullException(nameof(usbWatcher));
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToExport => this.goToExport
                          ??
                  (this.goToExport = new DelegateCommand(
                      this.ShowExport, this.CanShowExport));

        public List<MachineError> MachineErrors
        {
            get => this.machineErrors;
            set => this.SetProperty(ref this.machineErrors, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChange -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Dispose();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Start();

#if DEBUG
            this.exportableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
#endif

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                var lst = await this.machineErrorsWebService.GetAllAsync();
                this.MachineErrors = lst.OrderByDescending(o => o.OccurrenceDate).ToList();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CanShowExport()
        {
            return this.AvailableDrives.Any();
        }

        private void ShowExport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.About.ALARMSEXPORT,
                this.MachineErrors,
                trackCurrentView: true);
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangeEventArgs e)
        {
            // exportable drives
            var drives = ((UsbWatcherService)sender).Drives;
            try
            {
                this.exportableDrives = new ReadOnlyCollection<DriveInfo>(drives.ToList());
            }
            catch (Exception ex)
            {
                var exc = ex;
            }
            this.RaisePropertyChanged(nameof(this.AvailableDrives));
            this.goToExport?.RaiseCanExecuteChanged();

            if (this.exportableDrives != null)
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.ExportableDeviceDetected"));
            }
            else
            {
                this.ClearNotifications();
            }
        }

        #endregion
    }
}
