using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private bool canInputCellId;

        private bool canInputHeight;

        private bool canInputLoadingUnitId;

        private IEnumerable<Cell> cells;

        private int? inputCellId;

        private double? inputHeight;

        private int? inputLoadingUnitId;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingToLoadingUnit;

        private bool isUseWeightControl;

        private LoadingUnit loadingUnitInCell;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToHeightCommand;

        private DelegateCommand moveToLoadingUnitHeightCommand;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        private DelegateCommand setWeightControlCommand;

        #endregion

        #region Properties

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public bool CanInputHeight
        {
            get => this.canInputHeight;
            private set => this.SetProperty(ref this.canInputHeight, value);
        }

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set => this.SetProperty(ref this.inputCellId, value, this.InputCellIdPropertyChanged);
        }

        public double? InputHeight
        {
            get => this.inputHeight;
            set => this.SetProperty(ref this.inputHeight, value, this.RaiseCanExecuteChanged);
        }

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set => this.SetProperty(ref this.inputLoadingUnitId, value, this.InputLoadingUnitIdPropertyChanged);
        }

        public bool IsElevatorMovingToCell
        {
            get => this.isElevatorMovingToCell;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToHeight
        {
            get => this.isElevatorMovingToHeight;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToHeight, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToLoadingUnit
        {
            get => this.isElevatorMovingToLoadingUnit;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToLoadingUnit, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            private set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public LoadingUnit LoadingUnitInCell
        {
            get => this.loadingUnitInCell;
            private set => this.SetProperty(ref this.loadingUnitInCell, value);
        }

        public ICommand MoveToCellHeightCommand =>
           this.moveToCellHeightCommand
           ??
           (this.moveToCellHeightCommand = new DelegateCommand(
               async () => await this.MoveToCellHeightAsync(),
               this.CanMoveToCellHeight));

        public ICommand MoveToHeightCommand =>
                   this.moveToHeightCommand
           ??
           (this.moveToHeightCommand = new DelegateCommand(
               async () => await this.MoveToHeightAsync(),
               this.CanMoveToHeight));

        public ICommand MoveToLoadingUnitHeightCommand =>
                   this.moveToLoadingUnitHeightCommand
           ??
           (this.moveToLoadingUnitHeightCommand = new DelegateCommand(
               async () => await this.MoveToLoadingUnitHeightAsync(),
               this.CanMoveToLoadingUnitHeight));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            private set
            {
                if (this.SetProperty(ref this.selectedCell, value))
                {
                    if (this.selectedCell != null)
                    {
                        this.LoadingUnitInCell = this.loadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set
            {
                if (this.SetProperty(ref this.selectedLoadingUnit, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SetWeightControlCommand =>
           this.setWeightControlCommand
           ??
           (this.setWeightControlCommand = new DelegateCommand(
               () => this.SetWeightControl(),
               this.CanSetWeightControl));

        #endregion

        #region Methods

        private bool CanMoveToCellHeight()
        {
            return
               !this.KeyboardOpened
                &&
                this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                this.moveToCellPolicy?.IsAllowed == true;
        }

        private bool CanMoveToHeight()
        {
            return
                !this.KeyboardOpened
                &&
                this.InputHeight != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            return
               !this.KeyboardOpened
                &&
                this.SelectedLoadingUnit != null
                &&
                this.SelectedLoadingUnit.CellId != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanSetWeightControl()
        {
            return
                !this.KeyboardOpened
                &&
                (this.SelectedCell != null ||
                    (this.InputHeight.HasValue && this.InputHeight > 0))
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private void InputCellIdPropertyChanged()
        {
            if (this.cells is null)
            {
                return;
            }

            this.SelectedCell = this.inputCellId is null
                ? null
                : this.cells.SingleOrDefault(c => c.Id == this.inputCellId);

            if (this.SelectedCell != null)
            {
                this.InputHeight = this.SelectedCell.Position;
                this.InputLoadingUnitId = this.loadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id)?.Id;
            }

            if (this.SelectedLoadingUnit?.CellId is null)
            {
                this.LoadingUnitInCell = null;
            }
            else
            {
                this.LoadingUnitInCell = this.SelectedLoadingUnit;
            }

            this.RaiseCanExecuteChanged();
        }

        private void InputLoadingUnitIdPropertyChanged()
        {
            if (this.loadingUnits is null)
            {
                return;
            }

            this.SelectedLoadingUnit = this.inputLoadingUnitId == null
                ? null
                : this.loadingUnits.SingleOrDefault(c => c.Id == this.inputLoadingUnitId);

            if (this.SelectedLoadingUnit != null)
            {
                this.InputCellId = this.SelectedLoadingUnit.CellId;
            }
        }

        private async Task MoveToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                Debug.Assert(
                    this.SelectedCell != null,
                    "The selected cell should be specified.");

                await this.machineElevatorWebService.MoveToCellAsync(
                    this.SelectedCell.Id,
                    performWeighting: this.isUseWeightControl,
                    computeElongation: true);

                this.IsElevatorMovingToCell = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToCell = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveToHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.InputHeight.Value,
                    this.isUseWeightControl,
                    false);

                this.IsElevatorMovingToHeight = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToHeight = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveToLoadingUnitHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                Debug.Assert(this.SelectedLoadingUnit != null, "A loading unit should be selected.");
                Debug.Assert(this.SelectedLoadingUnit.Cell != null, "The selected loading unit should specify a cell.");

                await this.machineElevatorWebService.MoveToCellAsync(
                    this.SelectedLoadingUnit.Cell.Id,
                    performWeighting: this.isUseWeightControl,
                    computeElongation: true);

                this.IsElevatorMovingToLoadingUnit = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToLoadingUnit = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void SetWeightControl()
        {
            this.IsUseWeightControl = !this.IsUseWeightControl;
        }

        #endregion
    }
}
