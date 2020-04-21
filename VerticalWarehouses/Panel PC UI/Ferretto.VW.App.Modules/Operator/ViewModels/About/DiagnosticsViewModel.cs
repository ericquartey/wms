using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.App.Services.IO;
using Prism.Commands;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class DiagnosticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly UsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToLogsExport;

        #endregion

        #region Constructors

        public DiagnosticsViewModel(UsbWatcherService usbWatcher)
                    : base()
        {
            this.usbWatcher = usbWatcher ?? throw new ArgumentNullException(nameof(usbWatcher));
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToLogsExport => this.goToLogsExport
                          ??
                  (this.goToLogsExport = new DelegateCommand(
                      this.ShowLogsExport, this.CanShowLogsExport));

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

        private bool CanShowLogsExport()
        {
            return this.AvailableDrives.Any();
        }

        private void ShowLogsExport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.About.LOGSEXPORT,
                null,
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
            this.goToLogsExport?.RaiseCanExecuteChanged();

            if (this.exportableDrives != null)
            {
                this.ShowNotification(Resources.InstallationApp.ExportableDeviceDetected);
            }
            else
            {
                this.ClearNotifications();
            }
        }

        #endregion
    }
}
