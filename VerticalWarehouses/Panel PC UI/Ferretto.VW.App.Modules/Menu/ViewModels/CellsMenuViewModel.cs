using System;
using System.Configuration;
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
    internal sealed class CellsMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand bayFirstLoadingUnitCommand;

        private DelegateCommand cellPanelsCheckCommand;

        private DelegateCommand cellsBlockTuningCommand;

        private DelegateCommand cellsCommand;

        private DelegateCommand cellsHeightCheckCommand;

        #endregion

        #region Constructors

        public CellsMenuViewModel(
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
            Cells,

            CellPanelsCheck,

            CellsHeightCheck,

            CellsBlockTuning,

            BayFirstLoadingUnit,
        }

        #endregion

        #region Properties

        public ICommand BayFirstLoadingUnitCommand =>
            this.bayFirstLoadingUnitCommand
            ??
            (this.bayFirstLoadingUnitCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.BayFirstLoadingUnit),
                () => this.CanExecuteCommand() &&
                      (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public BaySetupStatus BaysSetupStatus { get; private set; }

        private SetupStepStatus CellPanelsCheck => this.SetupStatusCapabilities?.CellPanelsCheck ?? new SetupStepStatus();

        public ICommand CellPanelsCheckCommand =>
            this.cellPanelsCheckCommand
            ??
            (this.cellPanelsCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellPanelsCheck),
                () => this.CanExecuteCommand() &&
                      (this.CellPanelsCheck.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        /// <summary>
        /// obsolete
        /// </summary>
        public ICommand CellsBlockTuningCommand =>
            this.cellsBlockTuningCommand
            ??
            (this.cellsBlockTuningCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsBlockTuning),
                () => this.CanExecuteCommand() &&
                      (this.CellsHeightCheck.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())));

        public ICommand CellsCommand =>
            this.cellsCommand
            ??
            (this.cellsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Cells),
                () => this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy ||
                      this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded));

        private SetupStepStatus CellsHeightCheck => this.SetupStatusCapabilities?.CellsHeightCheck ?? new SetupStepStatus();

        /// <summary>
        /// obsolete
        /// </summary>
        public ICommand CellsHeightCheckCommand =>
            this.cellsHeightCheckCommand
            ??
            (this.cellsHeightCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsHeightCheck),
                () => this.CanExecuteCommand() &&
                      (this.CellsHeightCheck.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus()) &&
                      false));

        private SetupStepStatus FirstLoadingUnit => this.SetupStatusCapabilities?.LoadFirstDrawerTest ?? new SetupStepStatus();

        public bool IsBayFirstLoadingUnitProcedure => this.FirstLoadingUnit.IsCompleted && !this.FirstLoadingUnit.IsBypassed;

        public bool IsBayFirstLoadingUnitProcedureBypassed => this.FirstLoadingUnit.IsBypassed;

        public bool IsCellPanelsCheckProcedure => this.CellPanelsCheck.IsCompleted && !this.CellPanelsCheck.IsBypassed;

        public bool IsCellPanelsCheckProcedureBypassed => this.CellPanelsCheck.IsBypassed;

        public bool IsCellsHeightCheckProcedure => this.CellsHeightCheck.IsCompleted && !this.CellsHeightCheck.IsBypassed;

        public bool IsCellsHeightCheckProcedureBypassed => this.CellsHeightCheck.IsBypassed;

        public SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            await this.UpdateSetupStatusAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsCellPanelsCheckProcedure));
            this.RaisePropertyChanged(nameof(this.IsCellPanelsCheckProcedureBypassed));
            this.RaisePropertyChanged(nameof(this.IsCellsHeightCheckProcedure));
            this.RaisePropertyChanged(nameof(this.IsCellsHeightCheckProcedureBypassed));
            this.RaisePropertyChanged(nameof(this.IsBayFirstLoadingUnitProcedure));
            this.RaisePropertyChanged(nameof(this.IsBayFirstLoadingUnitProcedureBypassed));

            this.cellsCommand?.RaiseCanExecuteChanged();
            this.cellPanelsCheckCommand?.RaiseCanExecuteChanged();
            this.cellsHeightCheckCommand?.RaiseCanExecuteChanged();
            this.cellsBlockTuningCommand?.RaiseCanExecuteChanged();
            this.bayFirstLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand(Menu menu)
        {
            switch (menu)
            {
                case Menu.BayFirstLoadingUnit:
                    this.NavigationService.Appear(
                       nameof(Utils.Modules.Installation),
                       Utils.Modules.Installation.LOADFIRSTDRAWER,
                       data: null,
                       trackCurrentView: true);
                    break;

                case Menu.Cells:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CellsLoadingUnitsMenu.CELLES,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.CellPanelsCheck:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CELLPANELSCHECK,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.CellsHeightCheck:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CELLSHEIGHTCHECK,
                        data: null,
                        trackCurrentView: true);
                    break;

                case Menu.CellsBlockTuning:
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Installation),
                        Utils.Modules.Installation.CELLSSIDECONTROL,
                        data: null,
                        trackCurrentView: true);
                    break;
            }
        }

        private async Task UpdateSetupStatusAsync()
        {
            this.SetupStatusCapabilities = await this.machineSetupStatusWebService.GetAsync();

            switch (this.MachineService.BayNumber)
            {
                case BayNumber.BayOne:
                    this.BaysSetupStatus = this.SetupStatusCapabilities.Bay1;
                    break;

                case BayNumber.BayTwo:
                    this.BaysSetupStatus = this.SetupStatusCapabilities.Bay2;
                    break;

                case BayNumber.BayThree:
                    this.BaysSetupStatus = this.SetupStatusCapabilities.Bay3;
                    break;

                default:
                    throw new ArgumentException($"Bay {this.MachineService.BayNumber} not allowed", nameof(this.MachineService.BayNumber));
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
