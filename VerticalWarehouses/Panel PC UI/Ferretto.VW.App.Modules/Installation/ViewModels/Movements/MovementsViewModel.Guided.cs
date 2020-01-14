using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand closedShutterCommand;

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

        private string labelMoveToLoadunit;

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

        private BayPosition selectedBayPosition1;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private DelegateCommand unloadToBayCommand;

        private ActionPolicy unloadToBayPolicy;

        private DelegateCommand unloadToCellCommand;

        private ActionPolicy unloadToCellPolicy;

        #endregion

        #region Properties

        public bool BayIsShutterThreeSensors => this.MachineService.IsShutterThreeSensors;

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

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public int? InputCellId
        {
            get => this.inputCellId;
            set => this.SetProperty(ref this.inputCellId, value, this.InputCellIdPropertyChanged); // HACK: 2
        }

        public double? InputHeight
        {
            get => this.inputHeight;
            set => this.SetProperty(ref this.inputHeight, value, this.RaiseCanExecuteChanged);
        }

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set => this.SetProperty(ref this.inputLoadingUnitId, value, this.InputLoadingUnitIdPropertyChanged);    // HACK: 1-3
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
            private set => this.SetProperty(ref this.isShutterMoving, value);
        }

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set
            {
                if (this.SetProperty(ref this.isTuningBay, value))
                {
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
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
                    //this.RaisePropertyChanged(nameof(this.IsMoving));
                    //this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public string LabelMoveToLoadunit
        {
            get => this.labelMoveToLoadunit;
            set => this.SetProperty(ref this.labelMoveToLoadunit, value);
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
                () => this.CanMoveToBayPosition()));

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

        public BayPosition SelectedBayPosition1
        {
            get => this.selectedBayPosition1;
            private set
            {
                if (this.SetProperty(ref this.selectedBayPosition1, value))
                {
                    this.RaiseCanExecuteChanged();
                    //this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
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
                    // HACK: 1
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

        #endregion

        #region Methods

        protected void OnGuidedRaiseCanExecuteChanged()
        {
            this.CanInputLoadingUnitId =
                this.CanBaseExecute()
                &&
                this.LoadingUnits != null
                &&
                this.Cells != null;

            if (!this.IsMoving &&
                (this.HealthProbeService.HealthStatus == HealthStatus.Healthy ||
                 this.HealthProbeService.HealthStatus == HealthStatus.Degraded))
            {
                Debug.WriteLine("OnGuidedRaiseCanExecuteChanged:RefreshActionPoliciesAsync");
                Task.Run(async () => await this.RefreshActionPoliciesAsync()).GetAwaiter().GetResult();

                //this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();

                this.tuningBayCommand?.RaiseCanExecuteChanged();
                this.tuningChainCommand?.RaiseCanExecuteChanged();

                this.openShutterCommand?.RaiseCanExecuteChanged();
                this.intermediateShutterCommand?.RaiseCanExecuteChanged();
                this.closedShutterCommand?.RaiseCanExecuteChanged();

                this.moveCarouselDownCommand?.RaiseCanExecuteChanged();
                this.moveCarouselUpCommand?.RaiseCanExecuteChanged();

                //this.loadFromBayCommand?.RaiseCanExecuteChanged();
                //this.loadFromCellCommand?.RaiseCanExecuteChanged();
                //this.unloadToBayCommand?.RaiseCanExecuteChanged();
                //this.unloadToCellCommand?.RaiseCanExecuteChanged();

                this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
                this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

                //this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanCloseShutter()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsShutterMoving
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Open || this.SensorsService.ShutterSensors.MidWay));
        }

        private bool CanExecuteIntermediateCommand()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsShutterMoving
                &&
                this.BayIsShutterThreeSensors
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Open || this.SensorsService.ShutterSensors.Closed));
        }

        private bool CanLoadFromBay()
        {
            return
                this.CanBaseExecute()
                &&
                this.SelectedBayPosition() != null
                &&
                this.loadFromBayPolicy?.IsAllowed == true;
        }

        private bool CanLoadFromCell()
        {
            return
                this.CanBaseExecute()
                &&
                this.SelectedCell != null
                &&
                this.loadFromCellPolicy?.IsAllowed == true;
        }

        private bool CanMoveCarouselDown()
        {
            return
                this.CanBaseExecute()
                &&
                this.moveCarouselDownPolicy?.IsAllowed == true;
        }

        private bool CanMoveCarouselUp()
        {
            return
                this.CanBaseExecute()
                &&
                this.moveCarouselUpPolicy?.IsAllowed == true;
        }

        private bool CanMoveToBayPosition()
        {
            return
                this.CanBaseExecute()
                &&
                this.SelectedBayPosition() != null
                &&
                this.moveToBayPositionPolicy?.IsAllowed == true;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            var canMove =
                this.CanBaseExecute()
                &&
                this.SelectedLoadingUnit != null
                &&
                this.SelectedLoadingUnit.CellId != null
                &&
                this.moveToCellPolicy?.IsAllowed == true
                &&
                !this.SensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide;

            if (!canMove)
            {
                canMove =
                    this.CanBaseExecute()
                    &&
                    this.SelectedLoadingUnit != null
                    &&
                    this.MachineStatus.EmbarkedLoadingUnit != null
                    &&
                    this.SensorsService.Sensors.LuPresentInMachineSide
                    &&
                    this.SensorsService.Sensors.LuPresentInOperatorSide;
            }

            return canMove;
        }

        private bool CanOpenShutter()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsShutterMoving
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay));
        }

        private bool CanSelectBayPosition()
        {
            return this.CanBaseExecute();
        }

        private bool CanTuneBay()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsTuningBay
                &&
                this.SensorsService.Sensors.ACUBay1S3IND;
        }

        private bool CanTuningChain()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsTuningChain
                &&
                this.SensorsService.IsZeroChain
                &&
                !this.SensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanUnloadToBay()
        {
            return
                this.CanBaseExecute()
                &&
                this.SelectedBayPosition() != null
                &&
                this.unloadToBayPolicy?.IsAllowed == true;
        }

        private bool CanUnloadToCell()
        {
            return
                this.CanBaseExecute()
                &&
                (this.SelectedCell != null ||
                    (this.MachineStatus.ElevatorLogicalPosition != null &&
                     this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell))
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
            if (this.Cells is null)
            {
                return;
            }

            // HACK: 2
            this.selectedCell = this.inputCellId is null
                ? null
                : this.Cells.SingleOrDefault(c => c.Id == this.inputCellId);

            if (this.selectedCell != null)
            {
                this.inputHeight = this.SelectedCell.Position;
                this.InputLoadingUnitId = this.LoadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id)?.Id;
            }

            if (this.selectedLoadingUnit?.CellId is null)
            {
                this.loadingUnitInCell = null;
            }
            else
            {
                this.loadingUnitInCell = this.selectedLoadingUnit;
            }

            this.RaisePropertyChanged(nameof(this.InputHeight));
            this.RaisePropertyChanged(nameof(this.LoadingUnitInCell));
            this.RaiseCanExecuteChanged();
        }

        private void InputLoadingUnitIdPropertyChanged()
        {
            if (this.LoadingUnits is null)
            {
                return;
            }

            // seleziono il cassetto in base al valore dello spin edit
            // HACK: 1
            this.selectedLoadingUnit = this.inputLoadingUnitId == null
                ? null
                : this.LoadingUnits.SingleOrDefault(c => c.Id == this.inputLoadingUnitId);

            if (this.selectedLoadingUnit != null)
            {
                // valorizzo l'id della cella del cassetto selezionato nella view dei comandi manuali
                // Hack: 3
                this.InputCellId = this.selectedLoadingUnit.CellId;

                // Uso la proprietà per scatenare action sulla proprietà
                //this.RaisePropertyChanged(nameof(this.InputCellId));
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

                var selectedBayPosition = this.SelectedBayPosition();
                if (selectedBayPosition.LoadingUnit is null)
                {
                    await this.machineElevatorWebService.LoadFromBayAsync(selectedBayPosition.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.StartMovingLoadingUnitToBayAsync(selectedBayPosition.LoadingUnit.Id, LoadingUnitLocation.Elevator);
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

                this.InputHeight = this.SelectedBayPosition().Height;

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.SelectedBayPosition().Id,
                    computeElongation: true,
                    performWeighting: this.isUseWeightControl);

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

                if (this.MachineStatus.EmbarkedLoadingUnit != null
                    && this.MachineStatus.EmbarkedLoadingUnit.Cell is null)
                {
                    await this.machineElevatorWebService.MoveToFreeCellAsync(
                        this.MachineStatus.EmbarkedLoadingUnit.Id,
                        performWeighting: this.isUseWeightControl,
                        computeElongation: true);
                }
                else
                {
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

                        this.RefreshMachineInfo();

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.RefreshMachineInfo();

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
            this.RaiseCanExecuteChanged();
        }

        private void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.RaiseCanExecuteChanged();
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
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
            var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
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

                var selectedBayPosition = this.SelectedBayPosition();
                if (this.MachineStatus.EmbarkedLoadingUnit is null)
                {
                    await this.machineElevatorWebService.UnloadToBayAsync(selectedBayPosition.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(selectedBayPosition.Location, this.MachineStatus.EmbarkedLoadingUnit.Id);
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

                if (this.MachineStatus.EmbarkedLoadingUnit is null)
                {
                    await this.machineElevatorWebService.UnloadToCellAsync(this.SelectedCell.Id);
                }
                else
                {
                    if (this.MachineStatus.LogicalPositionId.HasValue)
                    {
                        await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(LoadingUnitLocation.LoadUnit, this.MachineStatus.LogicalPositionId, this.MachineStatus.EmbarkedLoadingUnit.Id);
                    }
                    else
                    {
                        await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(LoadingUnitLocation.LoadUnit, null, this.MachineStatus.EmbarkedLoadingUnit.Id);
                    }
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
