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
    internal sealed class DiagnosticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<DriveInfo> availableDrives = Array.Empty<DriveInfo>();

        private DelegateCommand goToLogsExport;

        private MachineStatistics lastServiceStatistics;

        private MachineStatistics totalStatistics;

        #endregion

        #region Constructors

        public DiagnosticsViewModel(
            IUsbWatcherService usbWatcher,
            IMachineServicingWebService machineServicingWebService)
            : base()
        {
            this.usbWatcher = usbWatcher ?? throw new ArgumentNullException(nameof(usbWatcher));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
        }

        #endregion

        #region Properties

        public IEnumerable<DriveInfo> AvailableDrives
        {
            get => this.availableDrives;
            set => this.SetProperty(ref this.availableDrives, value);
        }

        public ICommand GoToLogsExport => this.goToLogsExport
                          ??
                  (this.goToLogsExport = new DelegateCommand(
                      this.ShowLogsExport, this.CanShowLogsExport));

        public MachineStatistics LastServiceStatistics
        {
            get => this.lastServiceStatistics;
            set => this.SetProperty(ref this.lastServiceStatistics, value);
        }

        public MachineStatistics TotalStatistics
        {
            get => this.totalStatistics;
            set => this.SetProperty(ref this.totalStatistics, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.OnUsbDrivesChanged;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = true;

                this.usbWatcher.DrivesChanged += this.OnUsbDrivesChanged;
                this.usbWatcher.Enable();

#if DEBUG
                this.AvailableDrives = new ReadOnlyCollection<DriveInfo>(DriveInfo.GetDrives().ToList());
#endif
            }
            catch
            {
                // do nothing
            }

            try
            {
                var lastServicing = await this.machineServicingWebService.GetLastValidAsync();

                if (lastServicing != null)
                {
                    this.LastServiceStatistics = lastServicing.MachineStatistics;

                    this.RaisePropertyChanged(nameof(this.LastServiceStatistics));
                }

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

                this.TotalStatistics.TotalHorizontalAxisCycles = allServicing.Select(s => s.MachineStatistics.TotalHorizontalAxisCycles).Sum();
                this.TotalStatistics.TotalVerticalAxisCycles = allServicing.Select(s => s.MachineStatistics.TotalVerticalAxisCycles).Sum();

                foreach (var time in allServicing)
                {
                    this.TotalStatistics.TotalMissionTime += time.MachineStatistics.TotalMissionTime;
                }

                this.RaisePropertyChanged(nameof(this.TotalStatistics));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanShowLogsExport()
        {
            return this.AvailableDrives.Any();
        }

        private void OnUsbDrivesChanged(object sender, DrivesChangedEventArgs e)
        {
            this.AvailableDrives = this.usbWatcher.Drives;

            this.goToLogsExport?.RaiseCanExecuteChanged();

            if (this.AvailableDrives.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.ExportableDeviceDetected"));
            }
            else
            {
                this.ClearNotifications();
            }
        }

        private void ShowLogsExport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.About.LOGSEXPORT,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
