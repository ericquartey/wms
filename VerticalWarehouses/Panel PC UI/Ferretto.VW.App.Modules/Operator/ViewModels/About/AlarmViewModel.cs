using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
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

        private readonly DelegateCommand goToExport;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private List<MachineError> machineErrors;

        #endregion

        #region Constructors

        public AlarmViewModel(
            IMachineErrorsWebService machineErrorsWebService,
            IUsbWatcherService usbWatcher)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.usbWatcher = usbWatcher ?? throw new ArgumentNullException(nameof(usbWatcher));

            this.goToExport = new DelegateCommand(this.ShowExport, this.CanShowExport);
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.availableDrives;

        public ICommand GoToExport => this.goToExport;

        public List<MachineError> MachineErrors
        {
            get => this.machineErrors;
            set => this.SetProperty(ref this.machineErrors, value);
        }

        #endregion

        #region Methods

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

#if DEBUG
            this.availableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
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

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.availableDrives = this.usbWatcher.Drives;

            this.RaisePropertyChanged(nameof(this.AvailableDrives));
            this.goToExport?.RaiseCanExecuteChanged();

            if (this.availableDrives != null)
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
