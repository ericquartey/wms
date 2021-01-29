using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadingUnitsMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand extractionLoadingUnitsCommand;

        private DelegateCommand insertionLoadingUnitsCommand;

        private DelegateCommand loadingUnitsBayToBayCommand;

        private DelegateCommand loadingUnitsCommand;

        private DelegateCommand moveLoadingUnitsCommand;

        private DelegateCommand testCompleteCommand;

        #endregion

        #region Constructors

        public LoadingUnitsMenuViewModel(
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
            LoadingUnits,

            InsertionLoadingUnits,

            MoveLoadingUnits,

            ExtractionLoadingUnits,

            LoadingUnitsBayToBay,

            TestComplete,
        }

        #endregion

        #region Properties

        public ICommand ExtractionLoadingUnitsCommand =>
            this.extractionLoadingUnitsCommand
            ??
            (this.extractionLoadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.ExtractionLoadingUnits),
                () => CanExecuteOperation(Menu.ExtractionLoadingUnits)));

        public ICommand InsertionLoadingUnitsCommand =>
            this.insertionLoadingUnitsCommand
            ??
            (this.insertionLoadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.InsertionLoadingUnits),
                () => this.CanExecuteOperation(Menu.InsertionLoadingUnits)));

        public ICommand LoadingUnitsBayToBayCommand =>
            this.loadingUnitsBayToBayCommand
            ??
            (this.loadingUnitsBayToBayCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.LoadingUnitsBayToBay),
                () => this.CanExecuteOperation(Menu.LoadingUnitsBayToBay)));

        public ICommand LoadingUnitsCommand =>
            this.loadingUnitsCommand
            ??
            (this.loadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.LoadingUnits),
                () => (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded)));

        public ICommand MoveLoadingUnitsCommand =>
            this.moveLoadingUnitsCommand
            ??
            (this.moveLoadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.MoveLoadingUnits),
                () => this.CanExecuteOperation(Menu.MoveLoadingUnits)));

        public SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        public ICommand TestCompleteCommand =>
            this.testCompleteCommand
            ??
            (this.testCompleteCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestComplete),
                () => this.CanExecuteOperation(Menu.TestComplete)));

        private SetupStepStatus VerticalOriginCalibration => this.SetupStatusCapabilities?.VerticalOriginCalibration ?? new SetupStepStatus();

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            await this.UpdateSetupStatusAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.extractionLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.insertionLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.loadingUnitsCommand?.RaiseCanExecuteChanged();
            this.moveLoadingUnitsCommand?.RaiseCanExecuteChanged();
            this.testCompleteCommand?.RaiseCanExecuteChanged();
            this.loadingUnitsBayToBayCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteOperation(Menu menu)
        {
            switch (menu)
            {
                case Menu.ExtractionLoadingUnits:
                case Menu.InsertionLoadingUnits:
                case Menu.MoveLoadingUnits:
                case Menu.LoadingUnitsBayToBay:
                    switch (this.MachineService.BayNumber)
                    {
                        case BayNumber.BayOne:
                        default:
                            return this.MachineModeService.MachinePower == MachinePowerState.Powered &&
                              (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded) &&
                              (this.MachineModeService.MachineMode == MachineMode.Manual ||
                              this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations) &&
                              this.VerticalOriginCalibration.IsCompleted;

                        case BayNumber.BayTwo:
                            return this.MachineModeService.MachinePower == MachinePowerState.Powered &&
                              (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded) &&
                              (this.MachineModeService.MachineMode == MachineMode.Manual2 ||
                              this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations2) &&
                              this.VerticalOriginCalibration.IsCompleted;

                        case BayNumber.BayThree:
                            return this.MachineModeService.MachinePower == MachinePowerState.Powered &&
                              (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy || this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded) &&
                              (this.MachineModeService.MachineMode == MachineMode.Manual3 ||
                              this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations3) &&
                              this.VerticalOriginCalibration.IsCompleted;
                    }

                case Menu.TestComplete:
                    return this.CanExecuteCommand() &&
                      this.MachineService.IsHoming;

                default:
                    return this.CanExecuteCommand();
            }
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.LoadingUnits:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.CellsLoadingUnitsMenu.LOADINGUNITS,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.InsertionLoadingUnits:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOCELL,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.MoveLoadingUnits:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOCELL,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.ExtractionLoadingUnits:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMCELLTOBAY,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.LoadingUnitsBayToBay:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.LoadingUnits.LOADINGUNITFROMBAYTOBAY,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.TestComplete:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.FULLTEST,
                       data: null,
                       trackCurrentView: true);
                    break;
            }
        }

        private async Task UpdateSetupStatusAsync()
        {
            try
            {
                this.SetupStatusCapabilities = await this.machineSetupStatusWebService.GetAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
