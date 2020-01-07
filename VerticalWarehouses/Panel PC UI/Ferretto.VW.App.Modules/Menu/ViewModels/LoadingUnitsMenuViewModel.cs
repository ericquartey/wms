using System;
using System.Collections.Generic;
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
    internal sealed class LoadingUnitsMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand extractionLoadingUnitsCommand;

        private DelegateCommand insertionLoadingUnitsCommand;

        private DelegateCommand loadingUnitsBayToBayCommand;

        private DelegateCommand loadingUnitsCommand;

        private DelegateCommand moveLoadingUnitsCommand;

        private DelegateCommand testCompleteCommand;

        #endregion

        #region Constructors

        public LoadingUnitsMenuViewModel()
            : base()
        {
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
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual && this.MachineService.IsHoming));

        public ICommand InsertionLoadingUnitsCommand =>
            this.insertionLoadingUnitsCommand
            ??
            (this.insertionLoadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.InsertionLoadingUnits),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual && this.MachineService.IsHoming));

        public ICommand LoadingUnitsBayToBayCommand =>
            this.loadingUnitsBayToBayCommand
            ??
            (this.loadingUnitsBayToBayCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.LoadingUnitsBayToBay),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual && this.MachineService.IsHoming));

        public ICommand LoadingUnitsCommand =>
            this.loadingUnitsCommand
            ??
            (this.loadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.LoadingUnits),
                this.CanExecuteCommand));

        public ICommand MoveLoadingUnitsCommand =>
            this.moveLoadingUnitsCommand
            ??
            (this.moveLoadingUnitsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.MoveLoadingUnits),
                () => this.CanExecuteCommand() &&
                      this.MachineModeService.MachineMode == MachineMode.Manual && this.MachineService.IsHoming));

        public ICommand TestCompleteCommand =>
            this.testCompleteCommand
            ??
            (this.testCompleteCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.TestComplete),
                () => this.CanExecuteCommand() &&
                      this.MachineService.IsHoming &&
                      false));

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
            this.RaiseCanExecuteChanged();
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
                    break;
            }
        }

        #endregion
    }
}
