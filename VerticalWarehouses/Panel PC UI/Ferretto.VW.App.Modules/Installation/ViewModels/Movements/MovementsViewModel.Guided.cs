using System;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.CodeParser;
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

        private bool isPositionDownEnabled;

        private bool isPositionUpEnabled;

        private bool isShutterMoving;

        private bool isTuningExtBay;

        private bool isTuningBay;

        private bool isTuningChain;

        private bool isVerticalCalibration;

        private bool isUseWeightControl;

        private string labelMoveToLoadunit;

        private DelegateCommand loadFromBayCommand;

        private DelegateCommand loadFromCellCommand;

        private DelegateCommand verticalCalibrationCommand;

        private LoadingUnit loadingUnitInCell;

        private string log;

        private DelegateCommand moveCarouselDownCommand;

        private ActionPolicy moveCarouselDownPolicy;

        private DelegateCommand moveCarouselUpCommand;

        private ActionPolicy moveCarouselUpPolicy;

        private DelegateCommand moveExtBayTowardOperatorCommand;

        private ActionPolicy moveExtBayTowardOperatorPolicy;

        private DelegateCommand moveExtBayTowardMachineCommand;

        private ActionPolicy moveExtBayTowardMachinePolicy;

        private DelegateCommand moveExtBayMovementForInsertionCommand;

        private DelegateCommand moveExtBayMovementForExtractionCommand;

        private DelegateCommand moveToBayPositionCommand;

        private ActionPolicy moveToBayPositionPolicy;

        private ActionPolicy moveToCellPolicy;

        private ActionPolicy moveToHeightPolicy;

        private DelegateCommand moveToLoadingUnitHeightCommand;

        private DelegateCommand openShutterCommand;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private BayPosition selectedBayPosition1;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        private DelegateCommand tuningExtBayCommand;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private DelegateCommand unloadToBayCommand;

        private DelegateCommand unloadToCellCommand;

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

        public ICommand ExtBayTowardOperatorCommand =>
            this.moveExtBayTowardOperatorCommand
            ??
            (this.moveExtBayTowardOperatorCommand = new DelegateCommand(
                async () => await this.MoveExtBayTowardOperatorAsync(),
                this.CanMoveExtBayTowardOperator));

        public ICommand ExtBayTowardMachineCommand =>
            this.moveExtBayTowardMachineCommand
            ??
            (this.moveExtBayTowardMachineCommand = new DelegateCommand(
                async () => await this.MoveExtBayTowardMachineAsync(),
                this.CanMoveExtBayTowardMachine));

        public ICommand ExtBayMovementForInsertionCommand =>
            this.moveExtBayMovementForInsertionCommand
            ??
            (this.moveExtBayMovementForInsertionCommand = new DelegateCommand(
                async () => await this.MoveExtBayForInsertionAsync(),
                this.CanMoveExtBayForInsertion));

        public ICommand ExtBayMovementForExtractionCommand =>
            this.moveExtBayMovementForExtractionCommand
            ??
            (this.moveExtBayMovementForExtractionCommand = new DelegateCommand(
                async () => await this.MoveExtBayForExtractionAsync(),
                this.CanMoveExtBayForExtraction));

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public ICommand VerticalCalibrationCommand =>
            this.verticalCalibrationCommand
            ??
            (this.verticalCalibrationCommand = new DelegateCommand(
                async () => await this.VerticalCalibrationAsync(),
                this.CanVerticalCalibration));

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
            private set => this.SetProperty(ref this.isBusyLoadingFromBay, value);
        }

        public bool IsBusyLoadingFromCell
        {
            get => this.isBusyLoadingFromCell;
            private set => this.SetProperty(ref this.isBusyLoadingFromCell, value);
        }

        public bool IsBusyUnloadingToBay
        {
            get => this.isBusyUnloadingToBay;
            private set => this.SetProperty(ref this.isBusyUnloadingToBay, value);
        }

        public bool IsBusyUnloadingToCell
        {
            get => this.isBusyUnloadingToCell;
            private set => this.SetProperty(ref this.isBusyUnloadingToCell, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set => this.SetProperty(ref this.isElevatorMovingToBay, value);
        }

        public bool IsElevatorMovingToCell
        {
            get => this.isElevatorMovingToCell;
            private set => this.SetProperty(ref this.isElevatorMovingToCell, value);
        }

        public bool IsElevatorMovingToHeight
        {
            get => this.isElevatorMovingToHeight;
            private set => this.SetProperty(ref this.isElevatorMovingToHeight, value);
        }

        public bool IsElevatorMovingToLoadingUnit
        {
            get => this.isElevatorMovingToLoadingUnit;
            private set => this.SetProperty(ref this.isElevatorMovingToLoadingUnit, value);
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

        public bool IsPositionDownEnabled
        {
            get => this.isPositionDownEnabled;
            set => this.SetProperty(ref this.isPositionDownEnabled, value);
        }

        public bool IsPositionUpEnabled
        {
            get => this.isPositionUpEnabled;
            set => this.SetProperty(ref this.isPositionUpEnabled, value);
        }

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set => this.SetProperty(ref this.isShutterMoving, value);
        }

        public bool IsTuningExtBay
        {
            get => this.isTuningExtBay;
            private set => this.SetProperty(ref this.isTuningExtBay, value);
        }

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set => this.SetProperty(ref this.isTuningBay, value);
        }

        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set => this.SetProperty(ref this.isTuningChain, value);
        }

        public bool IsVerticalCalibration
        {
            get => this.isVerticalCalibration;
            private set => this.SetProperty(ref this.isVerticalCalibration, value);
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

        public ICommand TuningExtBayCommand =>
            this.tuningExtBayCommand
            ??
            (this.tuningExtBayCommand = new DelegateCommand(
                async () => await this.TuneExtBayAsync(),
                this.CanTuneExtBay));

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

        private void OnGuidedRaiseCanExecuteChanged()
        {
            this.CanInputLoadingUnitId =
                this.CanBaseExecute() &&
                this.LoadingUnits != null &&
                this.Cells != null &&
                this.MachineStatus.EmbarkedLoadingUnit is null;

            if (!this.IsMoving &&
                (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy ||
                 this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded))
            {
#if DEBUG
                var stackTrace = new StackTrace();
                var method1 = stackTrace.GetFrame(1).GetMethod().Name;
                var method2 = stackTrace.GetFrame(2).GetMethod().Name;
                var method3 = stackTrace.GetFrame(3).GetMethod().Name;
                Debug.WriteLine($"OnGuidedRaiseCanExecuteChanged: {method3} -> {method2} -> {method1}");
#endif

                Task.Run(async () => await this.RefreshActionPoliciesAsync().ConfigureAwait(false)).GetAwaiter().GetResult();

                this.tuningBayCommand?.RaiseCanExecuteChanged();
                this.tuningChainCommand?.RaiseCanExecuteChanged();

                this.tuningExtBayCommand?.RaiseCanExecuteChanged();

                this.openShutterCommand?.RaiseCanExecuteChanged();
                this.intermediateShutterCommand?.RaiseCanExecuteChanged();
                this.closedShutterCommand?.RaiseCanExecuteChanged();

                this.moveCarouselDownCommand?.RaiseCanExecuteChanged();
                this.moveCarouselUpCommand?.RaiseCanExecuteChanged();

                this.verticalCalibrationCommand?.RaiseCanExecuteChanged();

                this.moveExtBayTowardOperatorCommand?.RaiseCanExecuteChanged();
                this.moveExtBayTowardMachineCommand?.RaiseCanExecuteChanged();

                this.moveExtBayMovementForInsertionCommand?.RaiseCanExecuteChanged();
                this.moveExtBayMovementForExtractionCommand?.RaiseCanExecuteChanged();

                this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
                this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

                this.loadFromCellCommand?.RaiseCanExecuteChanged();
                this.unloadToCellCommand?.RaiseCanExecuteChanged();

                this.loadFromBayCommand?.RaiseCanExecuteChanged();
                this.unloadToBayCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanCloseShutter()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsShutterMoving
                && ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator)
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
                && ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator)
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Open || this.SensorsService.ShutterSensors.Closed));
        }

        private bool CanLoadFromBay()
        {
            // Check a condition for external bay
            var conditionOnExternalBay = true;
            if (this.HasBayExternal && this.SensorsService.IsLoadingUnitInMiddleBottomBay)
            {
                conditionOnExternalBay = false;
            }

            var selectedBayPosition = this.SelectedBayPosition();
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay &&
                   this.MachineStatus.LogicalPositionId == this.Bay.Id &&
                   this.CanBaseExecute() &&
                   this.IsPositionUpSelected == this.MachineStatus.BayPositionUpper &&
                   selectedBayPosition != null &&
                   selectedBayPosition.LoadingUnit != null &&
                   this.MachineStatus.EmbarkedLoadingUnit is null &&
                   conditionOnExternalBay;
        }

        private bool CanLoadFromCell()
        {
            if (this.Cells is null)
            {
                return false;
            }
            var cellPosition = this.Cells.FirstOrDefault(f => f.Id == this.MachineStatus?.LogicalPositionId);
            var res = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell &&
                   this.CanBaseExecute() &&
                   this.SelectedCell != null &&
                   !(cellPosition?.IsFree ?? true) &&
                   this.MachineStatus.EmbarkedLoadingUnit is null &&
                   this.LoadingUnitInCell != null;
            return res;
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

        private bool CanMoveExtBayTowardMachine()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        this.SensorsService.BayZeroChainUp &&
                        !this.SensorsService.BEDInternalBayBottom &&
                        !this.SensorsService.BEDExternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute() &&
                    !this.SensorsService.BayZeroChain;
            }
        }

        private bool CanMoveExtBayForInsertion()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        !this.SensorsService.BayZeroChain &&
                        !this.SensorsService.BEDInternalBayBottom &&
                        !this.SensorsService.BEDExternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute() &&
                    !this.SensorsService.BayZeroChain;
            }
        }

        private bool CanMoveExtBayForExtraction()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        !this.SensorsService.BayZeroChain &&
                        !this.SensorsService.BEDExternalBayBottom &&
                        !this.SensorsService.BEDInternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute();
            }
        }

        private bool CanMoveExtBayTowardOperator()
        {
            if (this.MachineService.Bay.IsDouble)
            {
                return this.CanBaseExecute() &&
                        this.SensorsService.BayZeroChain &&
                        !this.SensorsService.BEDExternalBayBottom &&
                        !this.SensorsService.BEDInternalBayTop;
            }
            else
            {
                return
                    this.CanBaseExecute() &&
                    this.SensorsService.BayZeroChain;
            }
        }

        private bool CanMoveToBayPosition()
        {
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                this.CanBaseExecute()
                &&
                this.SelectedBayPosition() != null
                &&
                this.moveToBayPositionPolicy?.IsAllowed == true;
        }

        private bool CanMoveToLoadingUnitHeight()
        {
            var canMove =
                (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter)
                &&
                this.CanBaseExecute()
                &&
                this.SelectedLoadingUnit != null
                &&
                this.SelectedLoadingUnit.CellId != null
                &&
                this.selectedLoadingUnit.Height > 0
                &&
                this.moveToCellPolicy?.IsAllowed == true
                &&
                !this.SensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.SensorsService.Sensors.LuPresentInOperatorSide
                &&
                string.IsNullOrEmpty(this.Error);

            if (!canMove)
            {
                canMove =
                    (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter)
                    &&
                    this.CanBaseExecute()
                    &&
                    this.SelectedLoadingUnit != null
                    &&
                    this.selectedLoadingUnit.Height > 0
                    &&
                    this.MachineStatus.EmbarkedLoadingUnit != null
                    &&
                    this.SensorsService.Sensors.LuPresentInMachineSide
                    &&
                    this.SensorsService.Sensors.LuPresentInOperatorSide
                    &&
                    string.IsNullOrEmpty(this.Error);
            }

            return canMove;
        }

        private bool CanOpenShutter()
        {
            return
                this.CanBaseExecute()
                &&
                !this.IsShutterMoving
                && ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator)
                &&
                (this.SensorsService.ShutterSensors != null && (this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay));
        }

        private bool CanSelectBayPosition()
        {
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
               this.CanBaseExecute()
               &&
               this.moveToBayPositionPolicy?.IsAllowed == true;
        }

        private bool CanVerticalCalibration()
        {
            return this.CanBaseExecute() &&
                   !this.IsVerticalCalibration &&
                   !this.MachineService.MachineStatus.IsMoving &&
                   !this.MachineService.MachineStatus.IsMovingLoadingUnit &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanTuneExtBay()
        {
            return this.CanBaseExecute() &&
                   !this.IsTuningExtBay;
        }

        private bool CanTuneBay()
        {
            return this.CanBaseExecute() &&
                   !this.IsTuningBay &&
                   this.MachineStatus.LoadingUnitPositionDownInBay is null &&
                   this.SensorsService.Sensors.ACUBay1S3IND;
        }

        private bool CanTuningChain()
        {
            return this.CanBaseExecute() &&
                   !this.IsVerticalCalibration &&
                   !this.MachineService.MachineStatus.IsMoving &&
                   !this.MachineService.MachineStatus.IsMovingLoadingUnit &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanUnloadToBay()
        {
            var selectedBayPosition = this.SelectedBayPosition();
            var res = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                   this.CanBaseExecute() &&
                   this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Bay &&
                   this.MachineStatus.LogicalPositionId == this.Bay.Id &&
                   this.IsPositionUpSelected == this.MachineStatus.BayPositionUpper &&
                   selectedBayPosition != null &&
                   selectedBayPosition.LoadingUnit == null &&
                   this.MachineStatus.EmbarkedLoadingUnit != null;

            return res;
        }

        private bool CanUnloadToCell()
        {
            var res = (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed || this.SensorsService.ShutterSensors.MidWay || !this.HasShutter) &&
                this.CanBaseExecute()
                &&
                (this.SelectedCell != null ||
                    (this.MachineStatus.ElevatorLogicalPosition != null &&
                     this.MachineStatus.ElevatorPositionType == CommonUtils.Messages.Enumerations.ElevatorPositionType.Cell))
                &&
                this.MachineStatus.EmbarkedLoadingUnit != null;

            return res;
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
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
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

                if (this.selectedLoadingUnit.CellId != null)
                {
                    this.InputCellId = this.selectedLoadingUnit.CellId;
                }
                // Uso la proprietà per scatenare action sulla proprietà
                //this.RaisePropertyChanged(nameof(this.InputCellId));
            }
        }

        private async Task VerticalCalibrationAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureWebService.StartAsync();

                this.IsVerticalCalibration = true;

                this.IsExecutingProcedure = true;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
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
                if (selectedBayPosition.LoadingUnit != null)
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

                if (this.LoadingUnitInCell != null)
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
                this.loadFromCellCommand?.RaiseCanExecuteChanged();
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

        private async Task MoveExtBayForInsertionAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MovementForInsertionAsync(false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MovementForInsertionAsync(true);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MovementForInsertionAsync(false);
                }
                this.IsExternalBayMoving = true;
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

        private async Task MoveExtBayForExtractionAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MovementForExtractionAsync(false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MovementForExtractionAsync(true);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MovementForExtractionAsync(false);
                }

                this.IsExternalBayMoving = true;
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

        private async Task MoveExtBayTowardMachineAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    //var selectedBayPosition = this.SelectedBayPosition();
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardMachine, true);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardMachine, false);
                    }
                    else
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardMachine, this.IsPositionUpSelected);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MoveAssistedAsync(ExternalBayMovementDirection.TowardMachine);
                }
                this.IsExternalBayMoving = true;
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

        private async Task MoveExtBayTowardOperatorAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                if (this.MachineService.Bay.IsDouble)
                {
                    //var selectedBayPosition = this.SelectedBayPosition();
                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardOperator, false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardOperator, true);
                    }
                    else
                    {
                        await this.machineExternalBayWebService.MoveAssistedExternalBayAsync(ExternalBayMovementDirection.TowardOperator, this.IsPositionUpSelected);
                    }
                }
                else
                {
                    await this.machineExternalBayWebService.MoveAssistedAsync(ExternalBayMovementDirection.TowardOperator);
                }
                this.IsExternalBayMoving = true;
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

        private void OnGuidedPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
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

                        if (message.Data?.MovementMode == CommonUtils.Messages.Enumerations.MovementMode.ExtBayChain)
                        {
                            this.IsExternalBayMoving = false;
                        }

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

        private async void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.RaiseCanExecuteChanged();

            await this.MoveToBayPositionAsync();
        }

        private async void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.RaiseCanExecuteChanged();

            await this.MoveToBayPositionAsync();
        }

        private async Task TuneExtBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, InstallationApp.ExtBayCalibration, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineExternalBayWebService.FindZeroAsync();
                    this.IsTuningExtBay = true;
                    this.IsExecutingProcedure = true;
                }
            }
            catch (Exception ex)
            {
                this.IsTuningExtBay = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.BayCalibration"), DialogType.Question, DialogButtons.YesNo);
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
            var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.ChainCalibration"), DialogType.Question, DialogButtons.YesNo);
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
                        await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(LoadingUnitLocation.Elevator, this.MachineStatus.LogicalPositionId, this.MachineStatus.EmbarkedLoadingUnit.Id);
                    }
                    else
                    {
                        await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(LoadingUnitLocation.Elevator, null, this.MachineStatus.EmbarkedLoadingUnit.Id);
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
                this.unloadToCellCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
