using System;
using System.Collections.Generic;
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

        private LoadingUnit loadingUnitInCell;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToHeightCommand;

        private DelegateCommand moveToLoadingUnitHeightCommand;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

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

        public IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set
            {
                if (this.SetProperty(ref this.inputCellId, value)
                    &&
                    this.Cells != null)
                {
                    this.SelectedCell = value == null
                        ? null
                        : this.Cells.SingleOrDefault(c => c.Id == value);

                    this.InputHeight = this.SelectedCell?.Position ?? this.InputHeight;

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? InputHeight
        {
            get => this.inputHeight;
            set
            {
                if (this.SetProperty(ref this.inputHeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitId, value)
                    &&
                    this.LoadingUnits != null)
                {
                    this.SelectedLoadingUnit = value == null
                        ? null
                        : this.LoadingUnits.SingleOrDefault(c => c.Id == value);

                    this.InputCellId = this.SelectedLoadingUnit?.CellId;

                    this.RaiseCanExecuteChanged();
                }
            }
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
                        this.LoadingUnitInCell = this.LoadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id);
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

        #endregion

        #region Methods

        private bool CanMoveToCellHeight()
        {
            return this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanMoveToHeight()
        {
            return this.InputHeight != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            return this.SelectedLoadingUnit != null
                &&
                this.SelectedLoadingUnit.CellId != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private async Task MoveToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.SelectedCell.Position,
                    this.procedureParameters.FeedRateAfterZero,
                    true);

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
                    this.procedureParameters.FeedRateAfterZero,
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

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.SelectedLoadingUnit.Cell?.Position ?? 0,
                    this.procedureParameters.FeedRateAfterZero,
                    false);

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

        #endregion
    }
}
