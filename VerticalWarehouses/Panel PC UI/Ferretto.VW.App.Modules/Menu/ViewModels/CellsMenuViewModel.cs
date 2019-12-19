using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand bayFirstLoadingUnitCommand;

        private DelegateCommand cellPanelsCheckCommand;

        private DelegateCommand cellsBlockTuningCommand;

        private DelegateCommand cellsCommand;

        private DelegateCommand cellsHeightCheckCommand;

        private bool isWaitingForResponse;

        #endregion

        #region Constructors

        public CellsMenuViewModel()
            : base(PresentationMode.Menu)
        {
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
                this.CanExecuteCommand));

        public ICommand CellPanelsCheckCommand =>
            this.cellPanelsCheckCommand
            ??
            (this.cellPanelsCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellPanelsCheck),
                this.CanExecuteCommand));

        public ICommand CellsBlockTuningCommand =>
            this.cellsBlockTuningCommand
            ??
            (this.cellsBlockTuningCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsBlockTuning),
                this.CanExecuteCommand));

        public ICommand CellsCommand =>
            this.cellsCommand
            ??
            (this.cellsCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.Cells),
                this.CanExecuteCommand));

        public ICommand CellsHeightCheckCommand =>
            this.cellsHeightCheckCommand
            ??
            (this.cellsHeightCheckCommand = new DelegateCommand(
                () => this.ExecuteCommand(Menu.CellsHeightCheck),
                this.CanExecuteCommand));

        public override EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
        }

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
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
                        Utils.Modules.Installation.CellsHeightCheck.STEP1,
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

        private void RaiseCanExecuteChanged()
        {
            this.cellsCommand?.RaiseCanExecuteChanged();
            this.cellPanelsCheckCommand?.RaiseCanExecuteChanged();
            this.cellsHeightCheckCommand?.RaiseCanExecuteChanged();
            this.cellsBlockTuningCommand?.RaiseCanExecuteChanged();
            this.bayFirstLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
