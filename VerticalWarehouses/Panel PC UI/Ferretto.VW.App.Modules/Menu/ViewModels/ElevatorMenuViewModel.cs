using System;
using System.Configuration;
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

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand beltBurnishingCommand;

        private DelegateCommand horizontalChainCalibration;

        private DelegateCommand horizontalResolutionCalibration;

        private DelegateCommand testDepositAndPickUpCommand;

        private DelegateCommand verticalOffsetCalibration;

        private DelegateCommand verticalOriginCalibration;

        private DelegateCommand verticalResolutionCalibration;

        private DelegateCommand weightAnalysisCommand;

        private DelegateCommand weightCalibration;

        private DelegateCommand weightMeasurement;

        #endregion

        #region Constructors

        public ElevatorMenuViewModel(
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
            BeltBurnishing,

            WeightAnalysis,

            WeightMeasurement,

            VerticalOffsetCalibration,

            VerticalResolutionCalibration,

            VerticalOriginCalibration,

            HorizontalResolutionCalibration,

            HorizontalChainCalibration,

            WeightCalibration,

            TestDepositAndPickUp,
        }

        #endregion

        #region Properties

        private SetupStepStatus BeltBurnishing => this.SetupStatusCapabilities?.BeltBurnishing ?? new SetupStepStatus();

        public ICommand BeltBurnishingCommand =>
            this.beltBurnishingCommand
            ??
            (this.beltBurnishingCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BeltBurnishing),
                () => this.CanExecuteCommand() &&
                      (this.BeltBurnishing.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public override EnableMask EnableMask => EnableMask.Any;

        private SetupStepStatus HorizontalChain => this.SetupStatusCapabilities?.HorizontalChainCalibration ?? new SetupStepStatus();

        public ICommand HorizontalChainCalibration =>
                    this.horizontalChainCalibration
            ??
            (this.horizontalChainCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.HorizontalChainCalibration),
                () => this.CanExecuteCommand() &&
                      (this.HorizontalChain.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        private SetupStepStatus HorizontalResolution => this.SetupStatusCapabilities?.HorizontalResolutionCalibration ?? new SetupStepStatus();

        public ICommand HorizontalResolutionCalibration =>
                    this.horizontalResolutionCalibration
            ??
            (this.horizontalResolutionCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.HorizontalResolutionCalibration),
                () => this.CanExecuteCommand() &&
                      (this.HorizontalResolution.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public bool IsBeltBurnishing => this.BeltBurnishing.IsCompleted && !this.BeltBurnishing.IsBypassed;

        public bool IsBeltBurnishingBypassed => this.BeltBurnishing.IsBypassed;

        public bool IsHoming => this.VerticalOriginCalibration.IsCompleted;

        public bool IsHorizChainCompleted => this.HorizontalChain.IsCompleted && !this.BeltBurnishing.IsBypassed;

        public bool IsHorizontalChainBypassed => this.HorizontalChain.IsBypassed;

        public bool IsHorizontalResolutionBypassed => this.HorizontalResolution.IsBypassed;

        public bool IsHorizontalResolutionCompleted => this.HorizontalResolution.IsCompleted;

        public bool IsTestDepositAndPickUpCompleted => this.TestDepositAndPickUp.IsCompleted;

        public bool IsVerticalOffsetProcedure => this.VerticalOffsetCalibration.IsCompleted;

        public bool IsVerticalResolutionCalibration => this.VerticalResolutionCalibration.IsCompleted;

        public SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        private SetupStepStatus TestDepositAndPickUp => this.SetupStatusCapabilities?.DepositAndPickUpTest ?? new SetupStepStatus();

        public ICommand TestDepositAndPickUpCommand =>
            this.testDepositAndPickUpCommand
            ??
            (this.testDepositAndPickUpCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestDepositAndPickUp),
                () => this.CanExecuteCommand() &&
                      (this.BeltBurnishing.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        private SetupStepStatus VerticalOffsetCalibration => this.SetupStatusCapabilities?.VerticalOffsetCalibration ?? new SetupStepStatus();

        public ICommand VerticalOffsetCalibrationCommand =>
                    this.verticalOffsetCalibration
            ??
            (this.verticalOffsetCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOffsetCalibration),
                () => this.CanExecuteCommand() &&
                      (this.VerticalOffsetCalibration.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        private SetupStepStatus VerticalOriginCalibration => this.SetupStatusCapabilities?.VerticalOriginCalibration ?? new SetupStepStatus();

        public ICommand VerticalOriginCalibrationCommand =>
            this.verticalOriginCalibration
            ??
            (this.verticalOriginCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalOriginCalibration),
                () => this.CanExecuteCommand()));

        private SetupStepStatus VerticalResolutionCalibration => this.SetupStatusCapabilities?.VerticalResolutionCalibration ?? new SetupStepStatus();

        public ICommand VerticalResolutionCalibrationCommand =>
            this.verticalResolutionCalibration
            ??
            (this.verticalResolutionCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.VerticalResolutionCalibration),
                () => this.CanExecuteCommand() &&
                      (this.VerticalResolutionCalibration.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public ICommand WeightAnalysisCommand =>
            this.weightAnalysisCommand
            ??
            (this.weightAnalysisCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightAnalysis),
                () => this.CanExecuteCommand()));

        public ICommand WeightCalibrationCommand =>
                    this.weightCalibration
            ??
            (this.weightCalibration = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightCalibration),
                () => this.CanExecuteCommand() &&
                      (this.VerticalResolutionCalibration.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public ICommand WeightMeasurementCommand =>
            this.weightMeasurement
            ??
            (this.weightMeasurement = new DelegateCommand(
                () => this.ExecuteCommand(Menu.WeightMeasurement),
                () => this.CanExecuteCommand() &&
                (this.VerticalOriginCalibration.IsCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

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
            this.RaisePropertyChanged(nameof(this.IsBeltBurnishingBypassed));
            this.RaisePropertyChanged(nameof(this.IsTestDepositAndPickUpCompleted));

            this.RaisePropertyChanged(nameof(this.IsHorizChainCompleted));
            this.RaisePropertyChanged(nameof(this.IsHorizontalChainBypassed));

            this.beltBurnishingCommand?.RaiseCanExecuteChanged();
            this.verticalOffsetCalibration?.RaiseCanExecuteChanged();
            this.verticalOriginCalibration?.RaiseCanExecuteChanged();
            this.verticalResolutionCalibration?.RaiseCanExecuteChanged();
            this.weightAnalysisCommand?.RaiseCanExecuteChanged();
            this.weightMeasurement?.RaiseCanExecuteChanged();
            this.testDepositAndPickUpCommand?.RaiseCanExecuteChanged();
            this.horizontalChainCalibration?.RaiseCanExecuteChanged();
            this.weightCalibration?.RaiseCanExecuteChanged();
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

                case Menu.HorizontalChainCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.HORIZONTALCHAINCALIBRATION,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.HorizontalResolutionCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.HORIZONTALRESOLUTIONCALIBRATION,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.WeightCalibration:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.WEIGHTCALIBRATION,
                       data: null,
                       trackCurrentView: true);
                    break;
            }
        }

        private async Task UpdateSetupStatusAsync()
        {
            this.SetupStatusCapabilities = await this.machineSetupStatusWebService.GetAsync();

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
