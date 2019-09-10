using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineCellsService machineCellsService;

        private bool canInputCellId;

        private bool canInputLoadingUnitId;

        private bool canInputQuote;

        private IEnumerable<Cell> cells;

        private int? inputCellId;

        private int? inputLoadingUnitId;

        private decimal? inputQuote;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingToLoadingUnit;

        private bool isElevatorMovingToQuote;

        private LoadingUnit loadingUnitInCell;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand moveToLoadingUnitHeightCommand;

        private DelegateCommand moveToQuoteHeightCommand;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        #endregion

        #region Properties

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public bool CanInputQuote
        {
            get => this.canInputQuote;
            private set => this.SetProperty(ref this.canInputQuote, value);
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
                }
            }
        }

        public decimal? InputQuote
        {
            get => this.inputQuote;
            set
            {
                if (this.SetProperty(ref this.inputQuote, value))
                {
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
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
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
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToQuote
        {
            get => this.isElevatorMovingToQuote;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToQuote, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public LoadingUnit LoadingUnitInCell
        {
            get => this.loadingUnitInCell;
            protected set => this.SetProperty(ref this.loadingUnitInCell, value);
        }

        public ICommand MoveToCellHeightCommand =>
           this.moveToCellHeightCommand
           ??
           (this.moveToCellHeightCommand = new DelegateCommand(
               async () => await this.MoveToCellHeightAsync(),
               this.CanMoveToCellHeight));

        public ICommand MoveToLoadingUnitHeightCommand =>
           this.moveToLoadingUnitHeightCommand
           ??
           (this.moveToLoadingUnitHeightCommand = new DelegateCommand(
               async () => await this.MoveToLoadingUnitHeightAsync(),
               this.CanMoveToLoadingUnitHeight));

        public ICommand MoveToQuoteHeightCommand =>
                   this.moveToQuoteHeightCommand
           ??
           (this.moveToQuoteHeightCommand = new DelegateCommand(
               async () => await this.MoveToQuoteHeightAsync(),
               this.CanMoveToQuoteHeight));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            protected set
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
            protected set
            {
                if (this.SetProperty(ref this.selectedLoadingUnit, value))
                {
                    this.LoadingUnitInCell = this.selectedLoadingUnit;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        protected IEnumerable<Cell> Cells
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

        #endregion

        #region Methods

        private bool CanMoveToCellHeight()
        {
            return this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving
                &&
                !this.IsElevatorMovingToCell
                &&
                !this.IsElevatorMovingToQuote
                &&
                !this.IsElevatorMovingToLoadingUnit;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            return this.SelectedLoadingUnit != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving
                &&
                !this.IsElevatorMovingToCell
                &&
                !this.IsElevatorMovingToQuote
                &&
                !this.IsElevatorMovingToLoadingUnit;
        }

        private bool CanMoveToQuoteHeight()
        {
            return this.InputQuote != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving
                &&
                !this.IsElevatorMovingToCell
                &&
                !this.IsElevatorMovingToQuote
                &&
                !this.IsElevatorMovingToLoadingUnit;
        }

        private async Task MoveToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorService.MoveToVerticalPositionAsync(
                    this.SelectedCell.Position,
                    FeedRateCategory.VerticalManualMovementsAfterZero);

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

        private async Task MoveToLoadingUnitHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorService.MoveToVerticalPositionAsync(
                    this.SelectedLoadingUnit.Cell?.Position ?? 0,
                    FeedRateCategory.VerticalManualMovementsAfterZero);

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

        private async Task MoveToQuoteHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorService.MoveToVerticalPositionAsync(
                    this.InputQuote.Value,
                    FeedRateCategory.VerticalManualMovementsAfterZero);

                this.IsElevatorMovingToQuote = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToQuote = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.Cells = await this.machineCellsService.GetAllAsync();
            }
            catch (Exception ex)
            {
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
