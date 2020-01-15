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
    internal sealed class ElevatorMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineBeltBurnishingProcedureWebService beltBurnishingProcedureWebService;

        private readonly IMachineVerticalOffsetProcedureWebService verticalOffsetProcedureWebService;

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService verticalResolutionCalibrationProcedureWebService;

        private DelegateCommand beltBurnishingCommand;

        private DelegateCommand testDepositAndPickUpCommand;

        private DelegateCommand verticalOffsetCalibration;

        private DelegateCommand verticalOriginCalibration;

        private DelegateCommand verticalResolutionCalibration;

        private DelegateCommand weightAnalysisCommand;

        private DelegateCommand weightMeasurement;

        #endregion

        #region Constructors

        public ElevatorMenuViewModel(
            IMachineVerticalResolutionCalibrationProcedureWebService verticalResolutionCalibrationProcedureWebService,
            IMachineVerticalOffsetProcedureWebService verticalOffsetProcedureWebService,
            IMachineBeltBurnishingProcedureWebService beltBurnishingProcedureWebService)
            : base()
        {
            this.verticalResolutionCalibrationProcedureWebService = verticalResolutionCalibrationProcedureWebService ?? throw new ArgumentNullException(nameof(verticalResolutionCalibrationProcedureWebService));
            this.verticalOffsetProcedureWebService = verticalOffsetProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOffsetProcedureWebService));
            this.beltBurnishingProcedureWebService = beltBurnishingProcedureWebService ?? throw new ArgumentNullException(nameof(beltBurnishingProcedureWebService));
        }

        #endregion

        #region Enums

        private enum Menu
        {
            BeltBurnishing,

            WeightAnalysis,

            WeightMeasurement,

            VerticalOffsetCalibration,

            VerticalResolutionCalibration,

            VerticalOriginCalibration,

            TestDepositAndPickUp,
        }

        #endregion

        #region Properties

        public ICommand BeltBurnishingCommand =>
            this.beltBurnishingCommand
            ??
            (this.beltBurnishingCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BeltBurnishing),
                () => this.CanExecuteCommand() &&
                      (this.MachineModeService.MachineMode == MachineMode.Manual ||
                       this.MachineModeService.MachineMode == MachineMode.Test) &&
                      ((this.MachineService.IsHoming &&
                      ((this.VerticalResolutionCalibrationProcedureParameters != null &&
                        this.VerticalResolutionCalibrationProcedureParameters.IsCompleted &&
                        this.VerticalOffsetProcedureParameters != null &&
                        this.VerticalOffsetProcedureParameters.IsCompleted) ||
                       true)) || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public RepeatedTestProcedure BeltBurnishingProcedureParameters { get; private set; }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBeltBurnishing => this.BeltBurnishingProcedureParameters?.IsCompleted ?? false;

        public bool IsHoming => this.MachineService.IsHoming;

        public bool IsVerticalOffsetProcedure => this.VerticalOffsetProcedureParameters?.IsCompleted ?? false;

        public bool IsVerticalResolutionCalibration => this.VerticalResolutionCalibrationProcedureParameters?.IsCompleted ?? false;

        public ICommand TestDepositAndPickUpCommand =>
            this.testDepositAndPickUpCommand
            ??
            (this.testDepositAndPickUpCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestDepositAndPickUp),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      this.MachineService.IsHoming &&
                      false));

        public ICommand VerticalOffsetCalibrationCommand =>
            this.verticalOffsetCalibration
            ??
            (this.verticalOffsetCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOffsetCalibration),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                //&&
                //this.VerticalResolutionCalibrationProcedureParameters != null &&
                //this.VerticalResolutionCalibrationProcedureParameters.IsCompleted
                ));

        public ICommand VerticalOriginCalibrationCommand =>
            this.verticalOriginCalibration
            ??
            (this.verticalOriginCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOriginCalibration),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual));

        public ICommand VerticalResolutionCalibrationCommand =>
            this.verticalResolutionCalibration
            ??
            (this.verticalResolutionCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalResolutionCalibration),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
            ));

        public ICommand WeightAnalysisCommand =>
            this.weightAnalysisCommand
            ??
            (this.weightAnalysisCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightAnalysis),
                () => this.CanExecuteCommand() &&
                (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
            ));

        public ICommand WeightMeasurementCommand =>
            this.weightMeasurement
            ??
            (this.weightMeasurement = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightMeasurement),
                () => this.CanExecuteCommand() &&
                (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
            ));

        protected OffsetCalibrationProcedure VerticalOffsetProcedureParameters { get; private set; }

        protected VerticalResolutionCalibrationProcedure VerticalResolutionCalibrationProcedureParameters { get; private set; }

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.UpdateSetupStatusAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsHoming));
            this.RaisePropertyChanged(nameof(this.IsVerticalOffsetProcedure));
            this.RaisePropertyChanged(nameof(this.IsVerticalResolutionCalibration));
            this.RaisePropertyChanged(nameof(this.IsBeltBurnishing));

            this.beltBurnishingCommand?.RaiseCanExecuteChanged();
            this.verticalOffsetCalibration?.RaiseCanExecuteChanged();
            this.verticalOriginCalibration?.RaiseCanExecuteChanged();
            this.verticalResolutionCalibration?.RaiseCanExecuteChanged();
            this.weightAnalysisCommand?.RaiseCanExecuteChanged();
            this.weightMeasurement?.RaiseCanExecuteChanged();
            this.testDepositAndPickUpCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BeltBurnishing:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.BELTBURNISHING,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.WeightAnalysis:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.WEIGHTANALYSIS,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.WeightMeasurement:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.Elevator.WeightCheck.STEP1,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalOffsetCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.VERTICALOFFSETCALIBRATION,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalResolutionCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.VerticalOriginCalibration:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.VERTICALORIGINCALIBRATION,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.TestDepositAndPickUp:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.Bays.DEPOSITANDPICKUPTEST,
                       data: null,
                       trackCurrentView: true);
                    break;
            }
        }

        private async Task UpdateSetupStatusAsync()
        {
            if (this.VerticalResolutionCalibrationProcedureParameters == null ||
                !this.VerticalResolutionCalibrationProcedureParameters.IsCompleted)
            {
                this.VerticalResolutionCalibrationProcedureParameters = await this.verticalResolutionCalibrationProcedureWebService.GetParametersAsync();
            }

            if (this.VerticalOffsetProcedureParameters == null ||
                !this.VerticalOffsetProcedureParameters.IsCompleted)
            {
                this.VerticalOffsetProcedureParameters = await this.verticalOffsetProcedureWebService.GetParametersAsync();
            }

            if (this.BeltBurnishingProcedureParameters == null ||
                !this.BeltBurnishingProcedureParameters.IsCompleted)
            {
                this.BeltBurnishingProcedureParameters = await this.beltBurnishingProcedureWebService.GetParametersAsync();
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
