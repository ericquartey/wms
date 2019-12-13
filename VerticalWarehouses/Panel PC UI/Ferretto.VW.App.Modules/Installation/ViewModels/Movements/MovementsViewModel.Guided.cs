using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Regions;
using IDialogService = Ferretto.VW.App.Controls.Interfaces.IDialogService;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private double? bayChainTargetPosition;

        private bool bayIsShutterThreeSensors;

        private DelegateCommand closedShutterCommand;

        private double? horizontalTargetPosition;

        private int? inputCellId;

        private double? inputHeight;

        private int? inputLoadingUnitId;

        private DelegateCommand intermediateShutterCommand;

        private bool isBusyLoadingFromBay;

        private bool isBusyLoadingFromCell;

        private bool isBusyUnloadingToBay;

        private bool isBusyUnloadingToCell;

        private bool isElevatorMovingToBay;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingToLoadingUnit;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private bool isShutterMoving;

        private bool isTuningBay;

        private bool isTuningChain;

        private bool isUseWeightControl;

        private DelegateCommand loadFromBayCommand;

        private ActionPolicy loadFromBayPolicy;

        private DelegateCommand loadFromCellCommand;

        private ActionPolicy loadFromCellPolicy;

        private LoadingUnit loadingUnitInCell;

        private string log;

        private DelegateCommand moveCarouselDownCommand;

        private ActionPolicy moveCarouselDownPolicy;

        private DelegateCommand moveCarouselUpCommand;

        private ActionPolicy moveCarouselUpPolicy;

        private DelegateCommand moveToBayPositionCommand;

        private ActionPolicy moveToBayPositionPolicy;

        private ActionPolicy moveToCellPolicy;

        private DelegateCommand moveToLoadingUnitHeightCommand;

        private DelegateCommand openShutterCommand;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private BayPosition selectedBayPosition;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private DelegateCommand unloadToBayCommand;

        private ActionPolicy unloadToBayPolicy;

        private DelegateCommand unloadToCellCommand;

        private ActionPolicy unloadToCellPolicy;

        private double? verticalTargetPosition;

        #endregion

        #region Properties

        public double? BayChainTargetPosition
        {
            get => this.bayChainTargetPosition;
            private set => this.SetProperty(ref this.bayChainTargetPosition, value);
        }

        public bool BayIsShutterThreeSensors
        {
            get => this.bayIsShutterThreeSensors;
            set => this.SetProperty(ref this.bayIsShutterThreeSensors, value);
        }

        public ICommand CarouselDownCommand =>
                    this.moveCarouselDownCommand
            ??
            (this.moveCarouselDownCommand = new DelegateCommand(
            async () => await this.MoveCarouselDownAsync(),
            this.CanMoveCarouselDown));

        public ICommand CarouselUpCommand =>
            this.moveCarouselUpCommand
            ??
            (this.moveCarouselUpCommand = new DelegateCommand(
            async () => await this.MoveCarouselUpAsync(),
            this.CanMoveCarouselUp));

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public double? HorizontalTargetPosition
        {
            get => this.horizontalTargetPosition;
            private set => this.SetProperty(ref this.horizontalTargetPosition, value);
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

        public ICommand IntermediateShutterCommand =>
                    this.intermediateShutterCommand
            ??
            (this.intermediateShutterCommand = new DelegateCommand(
                async () => await this.IntermediateShutterAsync(),
                this.CanExecuteIntermediateCommand));

        public bool IsBusyLoadingFromBay
        {
            get => this.isBusyLoadingFromBay;
            private set
            {
                if (this.SetProperty(ref this.isBusyLoadingFromBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyLoadingFromCell
        {
            get => this.isBusyLoadingFromCell;
            private set
            {
                if (this.SetProperty(ref this.isBusyLoadingFromCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyUnloadingToBay
        {
            get => this.isBusyUnloadingToBay;
            private set
            {
                if (this.SetProperty(ref this.isBusyUnloadingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyUnloadingToCell
        {
            get => this.isBusyUnloadingToCell;
            private set
            {
                if (this.SetProperty(ref this.isBusyUnloadingToCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
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

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            private set
            {
                if (this.SetProperty(ref this.isPositionDownSelected, value))
                {
                    this.IsPositionUpSelected = !this.IsPositionDownSelected;
                }
            }
        }

        public bool IsPositionUpSelected
        {
            get => this.isPositionUpSelected;
            private set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value))
                {
                    this.IsPositionDownSelected = !this.IsPositionUpSelected;
                }
            }
        }

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set => this.SetProperty(ref this.isShutterMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set
            {
                if (this.SetProperty(ref this.isTuningBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set
            {
                if (this.SetProperty(ref this.isTuningChain, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public ICommand LoadFromBayCommand => this.loadFromBayCommand
            ??
            (this.loadFromBayCommand = new DelegateCommand(
                async () => await this.LoadFromBayAsync(),
                this.CanLoadFromBay));

        public ICommand LoadFromCellCommand =>
            this.loadFromCellCommand
            ??
            (this.loadFromCellCommand = new DelegateCommand(
                async () => await this.LoadFromCellAsync(),
                this.CanLoadFromCell));

        public LoadingUnit LoadingUnitInCell
        {
            get => this.loadingUnitInCell;
            private set => this.SetProperty(ref this.loadingUnitInCell, value);
        }

        public string Log
        {
            get => this.log;
            set => this.SetProperty(ref this.log, value);
        }

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand(
                async () => await this.MoveToBayPositionAsync(),
                this.CanMoveToBayPosition));

        public ICommand MoveToLoadingUnitHeightCommand =>
                   this.moveToLoadingUnitHeightCommand
           ??
           (this.moveToLoadingUnitHeightCommand = new DelegateCommand(
               async () => await this.MoveToLoadingUnitHeightAsync(),
               this.CanMoveToLoadingUnitHeight));

        public ICommand OpenShutterCommand =>
            this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanOpenShutter));

        public ICommand SelectBayPositionDownCommand =>
                                                                                                                                                                            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(
                this.SelectBayPositionDown,
                this.CanSelectBayPosition));

        public ICommand SelectBayPositionUpCommand =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(
                this.SelectBayPositionUp,
                this.CanSelectBayPosition));

        public BayPosition SelectedBayPosition
        {
            get => this.selectedBayPosition;
            private set
            {
                if (this.SetProperty(ref this.selectedBayPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

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

        public ICommand TuningBayCommand =>
            this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuneBayAsync(),
                this.CanTuneBay));

        public ICommand TuningChainCommand =>
            this.tuningChainCommand
            ??
            (this.tuningChainCommand = new DelegateCommand(
                async () => await this.TuningChainAsync(),
                this.CanTuningChain));

        public ICommand UnloadToBayCommand =>
            this.unloadToBayCommand ?? (this.unloadToBayCommand =
            new DelegateCommand(
                async () => await this.UnloadToBayAsync(),
                this.CanUnloadToBay));

        public ICommand UnloadToCellCommand =>
            this.unloadToCellCommand
            ??
            (this.unloadToCellCommand = new DelegateCommand(
                async () => await this.UnloadToCellAsync(),
                this.CanUnloadToCell));

        public double? VerticalTargetPosition
        {
            get => this.verticalTargetPosition;
            private set => this.SetProperty(ref this.verticalTargetPosition, value);
        }

        #endregion

        #region Methods

        protected void OnGuidedRaiseCanExecuteChanged()
        {
            this.CanInputLoadingUnitId =
                !this.IsKeyboardOpened
                &&
                this.loadingUnits != null
                &&
                this.cells != null
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;

            this.RefreshActionPoliciesAsync().ConfigureAwait(false);

            this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();

            this.tuningBayCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();

            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.intermediateShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();

            this.moveCarouselDownCommand?.RaiseCanExecuteChanged();
            this.moveCarouselUpCommand?.RaiseCanExecuteChanged();

            this.loadFromBayCommand?.RaiseCanExecuteChanged();
            this.loadFromCellCommand?.RaiseCanExecuteChanged();
            this.unloadToBayCommand?.RaiseCanExecuteChanged();
            this.unloadToCellCommand?.RaiseCanExecuteChanged();

            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();
            this.moveToBayPositionCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
        }

        private bool CanCloseShutter()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.MidWay));
        }

        private bool CanExecuteIntermediateCommand()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                this.BayIsShutterThreeSensors
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.Closed));
        }

        private bool CanLoadFromBay()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.loadFromBayPolicy?.IsAllowed == true;
        }

        private bool CanLoadFromCell()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.SelectedCell != null
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.loadFromCellPolicy?.IsAllowed == true;
        }

        private bool CanMoveCarouselDown()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.moveCarouselDownPolicy?.IsAllowed == true;
        }

        private bool CanMoveCarouselUp()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.moveCarouselUpPolicy?.IsAllowed == true;
        }

        private bool CanMoveToBayPosition()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                this.moveToBayPositionPolicy?.IsAllowed == true;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            var canMove =
                !this.IsKeyboardOpened
                &&
                this.SelectedLoadingUnit != null
                &&
                (this.SelectedLoadingUnit.CellId != null)
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide;

            if (!canMove)
            {
                canMove =
                    !this.IsKeyboardOpened
                    &&
                    this.SelectedLoadingUnit != null
                    &&
                    this.sensorsService.EmbarkedLoadingUnit != null
                    &&
                    this.sensorsService.EmbarkedLoadingUnit.Id == this.SelectedLoadingUnit.Id
                    &&
                    !this.IsExecutingProcedure
                    &&
                    !this.IsWaitingForResponse
                    &&
                    !this.IsMoving
                    &&
                    this.sensorsService.Sensors.LuPresentInMachineSide
                    &&
                    this.sensorsService.Sensors.LuPresentInOperatorSide;
            }

            return canMove;
        }

        private bool CanOpenShutter()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Closed || this.sensorsService.ShutterSensors.MidWay));
        }

        private bool CanSelectBayPosition()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanTuneBay()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsTuningBay
                &&
                this.sensorsService.Sensors.ACUBay1S3IND;
        }

        private bool CanTuningChain()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsTuningChain
                &&
                this.sensorsService.IsZeroChain
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanUnloadToBay()
        {
            return
                !this.IsKeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                this.unloadToBayPolicy?.IsAllowed == true;
        }

        private bool CanUnloadToCell()
        {
            return
                !this.IsKeyboardOpened
                &&
                (this.SelectedCell != null ||
                    (this.SensorsService.ElevatorLogicalPosition != null && this.SensorsService.ElevatorLogicalPosition.Contains("Cella"))) // TODO: X Mirco, DA CAMBIARE!!!!!
                &&
                !this.IsMoving
                &&
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                (this.SelectedCell == null || this.unloadToCellPolicy?.IsAllowed == true);
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Closed);
                this.IsShutterMoving = true;
                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
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

        private async Task IntermediateShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Half);
                this.IsShutterMoving = true;
                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task LoadFromBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected");

                if (this.SelectedBayPosition.LoadingUnit is null)
                {
                    await this.machineElevatorWebService.LoadFromBayAsync(this.SelectedBayPosition.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.SelectedBayPosition.LoadingUnit.Id, LoadingUnitLocation.Elevator);
                }

                this.IsBusyLoadingFromBay = true;
                this.IsExecutingProcedure = true;
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

        private async Task LoadFromCellAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedCell != null,
                    "A cell should be selected");

                if (this.LoadingUnitInCell is null)
                {
                    await this.machineElevatorWebService.LoadFromCellAsync(this.SelectedCell.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(this.LoadingUnitInCell.Id, LoadingUnitLocation.Elevator);
                }

                this.IsBusyLoadingFromCell = true;
                this.IsExecutingProcedure = true;
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

        private async Task MoveCarouselDownAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAssistedAsync(VerticalMovementDirection.Down);
                this.IsCarouselMoving = true;
                this.IsExecutingProcedure = true;
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

        private async Task MoveCarouselUpAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAssistedAsync(VerticalMovementDirection.Up);
                this.IsCarouselMoving = true;
                this.IsExecutingProcedure = true;
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

        private async Task MoveToBayPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected.");

                this.InputHeight = this.SelectedBayPosition.Height;

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.SelectedBayPosition.Id,
                    computeElongation: true,
                    performWeighting: false);

                this.IsElevatorMovingToBay = true;
                this.IsExecutingProcedure = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToBay = false;

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

                if (this.SensorsService.EmbarkedLoadingUnit != null
                    && this.SensorsService.EmbarkedLoadingUnit.Id == this.SelectedLoadingUnit.Id
                    )
                {
                    await this.machineElevatorWebService.MoveToFreeCellAsync(
                        this.SelectedLoadingUnit.Id,
                        performWeighting: this.isUseWeightControl,
                        computeElongation: true);
                }
                else
                {
                    Debug.Assert(this.SelectedLoadingUnit.Cell != null, "The selected loading unit should specify a cell.");

                    await this.machineElevatorWebService.MoveToCellAsync(
                        this.SelectedLoadingUnit.Cell.Id,
                        performWeighting: this.isUseWeightControl,
                        computeElongation: true);
                }

                this.IsElevatorMovingToLoadingUnit = true;
                this.IsExecutingProcedure = true;
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

        private async Task OnGuidedPositioningOperationChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            if (!this.IsMovementsGuided)
            {
                return;
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        if (message.Data?.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.BayChain)
                        {
                            this.IsCarouselMoving = false;
                        }

                        await this.RefreshMachineInfoAsync();

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        await this.RefreshMachineInfoAsync();

                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private void OnGuidedShutterPositionChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (!this.IsMovementsGuided)
            {
                return;
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    {
                        this.IsShutterMoving = true;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsShutterMoving = false;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.OperationWarningOrError(message.Status, message.Description);
                        break;
                    }
            }
        }

        private async Task OpenShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Opened);
                this.IsShutterMoving = true;
                this.IsExecutingProcedure = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Min(pos => pos.Height));
        }

        private void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Max(pos => pos.Height));
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineCarouselWebService.FindZeroAsync();
                    this.IsTuningBay = true;
                    this.IsExecutingProcedure = true;
                }
            }
            catch (Exception ex)
            {
                this.IsTuningBay = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task TuningChainAsync()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsWaitingForResponse = true;
                    await this.machineElevatorWebService.SearchHorizontalZeroAsync();
                    this.IsTuningChain = true;
                    this.IsExecutingProcedure = true;
                }
                catch (Exception ex)
                {
                    this.IsTuningChain = false;

                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsWaitingForResponse = false;
                }
            }
        }

        private async Task UnloadToBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected");

                if (this.SensorsService.EmbarkedLoadingUnit is null)
                {
                    await this.machineElevatorWebService.UnloadToBayAsync(this.SelectedBayPosition.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(LoadingUnitLocation.Elevator, this.SelectedBayPosition.Id);
                }

                this.IsBusyUnloadingToBay = true;
                this.IsExecutingProcedure = true;
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

        private async Task UnloadToCellAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedCell != null,
                    "A cell should be selected");

                if (this.SensorsService.EmbarkedLoadingUnit is null)
                {
                    await this.machineElevatorWebService.UnloadToCellAsync(this.SelectedCell.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(LoadingUnitLocation.LoadingUnit, this.SelectedCell.Id, this.SensorsService.EmbarkedLoadingUnit.Id);
                }

                this.IsBusyUnloadingToCell = true;
                this.IsExecutingProcedure = true;
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
