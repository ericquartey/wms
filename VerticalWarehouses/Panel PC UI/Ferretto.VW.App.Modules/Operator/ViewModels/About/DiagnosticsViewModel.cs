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
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class DiagnosticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly UsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> exportableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToLogsExport;

        private MachineStatistics totalStatistics;

        private MachineStatistics lastServiceStatistics;

        #endregion

        #region Constructors

        public DiagnosticsViewModel(UsbWatcherService usbWatcher,
            IMachineServicingWebService machineServicingWebService)
                    : base()
        {
            this.usbWatcher = usbWatcher ?? throw new ArgumentNullException(nameof(usbWatcher));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives => this.exportableDrives;

        public ICommand GoToLogsExport => this.goToLogsExport
                          ??
                  (this.goToLogsExport = new DelegateCommand(
                      this.ShowLogsExport, this.CanShowLogsExport));

        public MachineStatistics TotalStatistics
        {
            get => this.totalStatistics;
            set => this.SetProperty(ref this.totalStatistics, value);
        }

        public MachineStatistics LastServiceStatistics
        {
            get => this.lastServiceStatistics;
            set => this.SetProperty(ref this.lastServiceStatistics, value);
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
            try
            {
                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = true;

                this.usbWatcher.DrivesChange += this.UsbWatcher_DrivesChange;
                this.usbWatcher.Start();

#if DEBUG
                this.exportableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
#endif

                var lastServicing = await this.machineServicingWebService.GetLastConfirmedAsync();

                this.LastServiceStatistics = lastServicing.MachineStatistics;

                this.RaisePropertyChanged(nameof(this.LastServiceStatistics));

                var allServicing = await this.machineServicingWebService.GetAllAsync();

                this.TotalStatistics = new MachineStatistics();

                this.TotalStatistics.TotalBayChainKilometers1 = allServicing.Select(s => s.MachineStatistics.TotalBayChainKilometers1).Sum();
                this.TotalStatistics.TotalBayChainKilometers2 = allServicing.Select(s => s.MachineStatistics.TotalBayChainKilometers2).Sum();
                this.TotalStatistics.TotalBayChainKilometers3 = allServicing.Select(s => s.MachineStatistics.TotalBayChainKilometers3).Sum();

                this.TotalStatistics.TotalVerticalAxisKilometers = allServicing.Select(s => s.MachineStatistics.TotalVerticalAxisKilometers).Sum();
                this.TotalStatistics.TotalHorizontalAxisKilometers = allServicing.Select(s => s.MachineStatistics.TotalHorizontalAxisKilometers).Sum();

                this.TotalStatistics.TotalLoadUnitsInBay1 = allServicing.Select(s => s.MachineStatistics.TotalLoadUnitsInBay1).Sum();
                this.TotalStatistics.TotalLoadUnitsInBay2 = allServicing.Select(s => s.MachineStatistics.TotalLoadUnitsInBay2).Sum();
                this.TotalStatistics.TotalLoadUnitsInBay3 = allServicing.Select(s => s.MachineStatistics.TotalLoadUnitsInBay3).Sum();

                foreach (var time in allServicing)
                {
                    this.TotalStatistics.TotalMissionTime += time.MachineStatistics.TotalMissionTime;
                }

                this.RaisePropertyChanged(nameof(this.TotalStatistics));
            }
            catch (Exception)
            {
            }
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
