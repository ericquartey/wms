using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BaysMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand bayControlCommand;

        private DelegateCommand bayHeightCommand;

        private DelegateCommand carouselCalibrationCommand;

        private DelegateCommand testShutterCommand;

        #endregion

        #region Constructors

        public BaysMenuViewModel(
            IMachineSetupStatusWebService machineSetupStatusWebService)
            : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService ?? throw new ArgumentNullException(nameof(machineSetupStatusWebService));

            this.SetupStatusCapabilities = new SetupStatusCapabilities();
        }

        #endregion

        #region Enums

        private enum Menu
        {
            BayControl,

            BayHeight,

            CarouselCalibration,

            TestShutter,
        }

        #endregion

        #region Properties

        public ICommand BayControlCommand =>
            this.bayControlCommand
            ??
            (this.bayControlCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayControl),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.BayControl.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public ICommand BayHeightCommand =>
            this.bayHeightCommand
            ??
            (this.bayHeightCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayHeight),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        //protected SetupStepStatus CarouselCalibration => this.SetupStatusCapabilities?.CarouselCalibration ?? new SetupStepStatus();

        public ICommand CarouselCalibrationCommand =>
                    this.carouselCalibrationCommand
            ??
            (this.carouselCalibrationCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CarouselCalibration),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
               (true || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBayControlCompleted => this.BayControl.IsCompleted;

        public bool IsBayHeightCompleted => false;

        public bool IsCarouselCalibrationVisible => this.MachineService.HasCarousel;

        public bool IsTestBayVisible => (this.MachineService.HasBayExternal || this.MachineService.HasCarousel);

        public bool IsTestShutterCompleted => this.BayShutter.IsCompleted;

        public SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        public ICommand TestShutterCommand =>
                    this.testShutterCommand
            ??
            (this.testShutterCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestShutter),
                () => this.CanExecuteCommand() &&
                      (this.MachineModeService.MachineMode == MachineMode.Manual || this.MachineModeService.MachineMode == MachineMode.Test) &&
                      (this.BayShutter.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        private SetupStepStatus VerticalOriginCalibration => this.SetupStatusCapabilities?.VerticalOriginCalibration ?? new SetupStepStatus();

        private SetupStepStatus BayControl => this.BaySetupStatus?.Check ?? new SetupStepStatus();

        private BaySetupStatus BaySetupStatus
        {
            get
            {
                BaySetupStatus res = null;

                switch (this.MachineService.BayNumber)
                {
                    case BayNumber.BayOne:
                        res = this.SetupStatusCapabilities?.Bay1;
                        break;

                    case BayNumber.BayTwo:
                        res = this.SetupStatusCapabilities?.Bay2;
                        break;

                    case BayNumber.BayThree:
                        res = this.SetupStatusCapabilities?.Bay3;
                        break;
                }

                return res;
            }
        }

        private SetupStepStatus BayShutter => this.BaySetupStatus?.Shutter ?? new SetupStepStatus();

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.RaisePropertyChanged(nameof(this.IsCarouselCalibrationVisible));

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.SetupStatusCapabilities = await this.machineSetupStatusWebService.GetAsync();

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsBayControlCompleted));
            this.RaisePropertyChanged(nameof(this.IsBayHeightCompleted));
            this.RaisePropertyChanged(nameof(this.IsTestShutterCompleted));

            this.bayControlCommand?.RaiseCanExecuteChanged();
            this.bayHeightCommand?.RaiseCanExecuteChanged();
            this.testShutterCommand?.RaiseCanExecuteChanged();
            this.carouselCalibrationCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BayControl:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.Bays.BAYCHECK,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.BayHeight:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.PROFILEHEIGHTCHECKVIEW,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.CarouselCalibration:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CAROUSELCALIBRATION,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.TestShutter:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.SHUTTERENDURANCETEST,
                        data: null,
                        trackCurrentView: true);
                    break;
            }
        }

        #endregion
    }
}
