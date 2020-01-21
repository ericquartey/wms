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
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        protected SetupStepStatus CellPanelsCheck => this.SetupStatusCapabilities?.CellPanelsCheck ?? new SetupStepStatus();

        public ICommand CellPanelsCheckCommand =>
            this.cellPanelsCheckCommand
            ??
            (this.cellPanelsCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellPanelsCheck),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.CellPanelsCheck.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public ICommand CellsBlockTuningCommand =>
            this.cellsBlockTuningCommand
            ??
            (this.cellsBlockTuningCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsBlockTuning),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.MachineService.IsHoming || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                ));

        public ICommand CellsCommand =>
            this.cellsCommand
            ??
            (this.cellsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Cells),
                this.CanExecuteCommand));

        protected SetupStepStatus CellsHeightCheck => this.SetupStatusCapabilities?.CellsHeightCheck ?? new SetupStepStatus();

        public ICommand CellsHeightCheckCommand =>
                    this.cellsHeightCheckCommand
            ??
            (this.cellsHeightCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsHeightCheck),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual &&
                      (this.CellsHeightCheck.CanBePerformed || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
               ));

        protected SetupStatusCapabilities SetupStatusCapabilities { get; private set; }

        #endregion

        #region Methods

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

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
                        //Utils.Modules.Installation.CellsHeightCheck.STEP1,
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

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
