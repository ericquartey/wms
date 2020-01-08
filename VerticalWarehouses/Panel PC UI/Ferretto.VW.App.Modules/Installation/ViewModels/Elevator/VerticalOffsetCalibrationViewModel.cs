using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum VerticalOffsetCalibrationStep
    {
        Start,

        CellMeasured,

        Confirm,
    }

    [Warning(WarningsArea.Installation)]
    public class VerticalOffsetCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService;

        private readonly IMachineVerticalOffsetProcedureWebService verticalOffsetWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private DelegateCommand applyCorrectionCommand;

        private double axisLowerBound;

        private double axisUpperBound;

        private int? currentCellId;

        private string currentError;

        private VerticalOffsetCalibrationStep currentStep;

        private double? currentVerticalOffset;

        private decimal? displacement;

        private DelegateCommand displacementCommand;

        private DelegateCommand moveToCellCommand;

        private DelegateCommand moveToCellMeasuredCommand;

        private DelegateCommand moveToCellPositioningCommand;

        private DelegateCommand moveToConfirmCommand;

        private DelegateCommand moveToStartPositionCommand;

        private double newDisplacement;

        private DelegateCommand reloadCommand;

        private Cell selectedCell;

        private double startPosition;

        private SubscriptionToken stepChangedToken;

        private double stepValue;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineVerticalOffsetProcedureWebService verticalOffsetWebService)
            : base(PresentationMode.Installer)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.verticalOffsetWebService = verticalOffsetWebService ?? throw new ArgumentNullException(nameof(verticalOffsetWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));

            this.CurrentStep = VerticalOffsetCalibrationStep.Start;
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                () => this.ApplyCorrectionAsync()));

        public double AxisLowerBound
        {
            get => this.axisLowerBound;
            set => this.SetProperty(ref this.axisLowerBound, value, this.RaiseCanExecuteChanged);
        }

        public double AxisUpperBound
        {
            get => this.axisUpperBound;
            set => this.SetProperty(ref this.axisUpperBound, value, this.RaiseCanExecuteChanged);
        }

        public int? CurrentCellId
        {
            get => this.currentCellId;
            set => this.SetProperty(ref this.currentCellId, value, () =>
            {
                this.Displacement = null;
                this.StepValue = 0;
                this.RaiseCanExecuteChanged();
            });
        }

        public VerticalOffsetCalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.RaiseCanExecuteChanged);
        }

        public double? CurrentVerticalOffset
        {
            get => this.currentVerticalOffset;
            set => this.SetProperty(ref this.currentVerticalOffset, value);
        }

        public decimal? Displacement
        {
            get => this.displacement;
            set => this.SetProperty(ref this.displacement, value);
        }

        public ICommand DisplacementCommand =>
            this.displacementCommand
            ??
            (this.displacementCommand = new DelegateCommand(
                async () => await this.DisplacementCommandAsync(),
                this.CanDisplacementCommand));

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            this[nameof(this.StartPosition)],
            this[nameof(this.CurrentCellId)],
            this[nameof(this.StepValue)]);

        public bool HasStepCellMeasured => this.currentStep is VerticalOffsetCalibrationStep.CellMeasured;

        public bool HasStepConfirm => this.currentStep is VerticalOffsetCalibrationStep.Confirm;

        public bool HasStepStart => this.currentStep is VerticalOffsetCalibrationStep.Start;

        public bool IsCanStartPosition =>
            this.CanBaseExecute() &&
            !this.SensorsService.IsLoadingUnitOnElevator;

        public bool IsCanStepValue =>
            this.CanBaseExecute() &&
            !this.SensorsService.IsLoadingUnitOnElevator;

        public ICommand MoveToCellCommand =>
            this.moveToCellCommand
            ??
            (this.moveToCellCommand = new DelegateCommand(
                async () => await this.StartAsync(this.SelectedCell.Position),
                this.CanMoveToCellCommand));

        public ICommand MoveToCellMeasuredCommand =>
            this.moveToCellMeasuredCommand
            ??
            (this.moveToCellMeasuredCommand = new DelegateCommand(
                () => this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured,
                this.CanMoveToCellMeasuredCommand));

        public ICommand MoveToCellPositioningCommand =>
            this.moveToCellPositioningCommand
            ??
            (this.moveToCellPositioningCommand = new DelegateCommand(
                () => this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured,
                this.CanToCellPositioning));

        public ICommand MoveToConfirmCommand =>
            this.moveToConfirmCommand
            ??
            (this.moveToConfirmCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep = VerticalOffsetCalibrationStep.Confirm;
                    this.NewDisplacement = this.CurrentVerticalOffset.Value + (double)this.Displacement.Value;
                },
                this.CanMoveToConfirm));

        public ICommand MoveToStartPositionCommand =>
            this.moveToStartPositionCommand
            ??
            (this.moveToStartPositionCommand = new DelegateCommand(
                async () => await this.StartAsync(this.StartPosition),
                this.CanMoveToStartPosition));

        public double NewDisplacement
        {
            get => this.newDisplacement;
            set => this.SetProperty(ref this.newDisplacement, value);
        }

        public ICommand ReloadCommand =>
            this.reloadCommand
            ??
            (this.reloadCommand = new DelegateCommand(
                () => this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            protected set => this.SetProperty(ref this.selectedCell, value, this.RaiseCanExecuteChanged);
        }

        public double StartPosition
        {
            get => this.startPosition;
            set => this.SetProperty(ref this.startPosition, value, this.RaiseCanExecuteChanged);
        }

        public double StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.StartPosition):
                        if (this.CurrentStep == VerticalOffsetCalibrationStep.Start &&
                            !this.IsMoving)
                        {
                            if (this.StartPosition < 0)
                            {
                                this.currentError = $"Start position must be strictly positive.";
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if ((this.StartPosition < this.axisLowerBound ||
                                this.StartPosition > this.axisUpperBound) &&
                                this.axisLowerBound > 0 &&
                                this.axisUpperBound > 0)
                            {
                                this.currentError = $"Start position out of ranhe axis ({this.AxisLowerBound} - {this.AxisUpperBound}).";
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            this.ClearNotifications();
                        }

                        break;

                    case nameof(this.CurrentCellId):
                        if (this.CurrentStep == VerticalOffsetCalibrationStep.CellMeasured &&
                            !this.IsMoving)
                        {
                            if (!this.CurrentCellId.HasValue)
                            {
                                this.currentError = $"Cell selected is required.";
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if (this.CurrentCellId < 1 ||
                                this.SelectedCell is null)
                            {
                                this.currentError = $"Cell selected not present.";
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            this.ClearNotifications();
                        }

                        break;

                    case nameof(this.StepValue):
                        if (this.CurrentStep == VerticalOffsetCalibrationStep.CellMeasured &&
                            !this.IsMoving)
                        {
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RetrieveVerticalOffset();

            var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();
            this.AxisUpperBound = procedureParameters.UpperBound;
            this.AxisLowerBound = procedureParameters.LowerBound;

            var procedureCalibrationParameters = await this.resolutionCalibrationWebService.GetParametersAsync();
            this.StartPosition = procedureCalibrationParameters.StartPosition;

            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case VerticalOffsetCalibrationStep.Start:
                    if (e.Next)
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured;
                    }

                    break;

                case VerticalOffsetCalibrationStep.CellMeasured:
                    if (e.Next)
                    {
                        this.ApplyCorrectionAsync();
                        this.CurrentStep = VerticalOffsetCalibrationStep.Confirm;
                    }
                    else
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.Start;
                    }

                    break;

                case VerticalOffsetCalibrationStep.Confirm:
                    if (!e.Next)
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured;
                    }

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            if (!this.IsVisible)
            {
                return;
            }

            base.RaiseCanExecuteChanged();

            if (this.CurrentCellId is null)
            {
                this.SelectedCell = this.MachineService.Cells.FirstOrDefault();
                this.CurrentCellId = this.SelectedCell?.Id;
            }
            else
            {
                this.SelectedCell = this.MachineService.Cells.SingleOrDefault(c => c.Id.Equals(this.CurrentCellId));
            }

            this.RaisePropertyChanged(nameof(this.HasStepStart));
            this.RaisePropertyChanged(nameof(this.HasStepCellMeasured));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));

            this.RaisePropertyChanged(nameof(this.IsMoving));
            this.RaisePropertyChanged(nameof(this.IsCanStartPosition));
            this.RaisePropertyChanged(nameof(this.IsCanStepValue));
            this.RaisePropertyChanged(nameof(this.SelectedCell));
            this.RaisePropertyChanged(nameof(this.CurrentCellId));

            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToStartPositionCommand?.RaiseCanExecuteChanged();
            this.moveToCellPositioningCommand?.RaiseCanExecuteChanged();
            this.moveToCellCommand?.RaiseCanExecuteChanged();
            this.moveToCellMeasuredCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmCommand?.RaiseCanExecuteChanged();
            this.displacementCommand?.RaiseCanExecuteChanged();
            this.reloadCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();
        }

        private async void ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOffsetWebService.CompleteAsync(this.NewDisplacement);

                await this.RetrieveVerticalOffset();

                this.Displacement = null;

                this.ShowNotification("Offset asse verticale aggiornato.", Services.Models.NotificationSeverity.Success);

                this.CurrentStep = VerticalOffsetCalibrationStep.Confirm;

                this.NavigationService.GoBack();
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

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private bool CanDisplacementCommand()
        {
            return this.CanBaseExecute() &&
                   this.StepValue != 0;
        }

        private bool CanMoveToCellCommand()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitOnElevator &&
                   !(this.SelectedCell is null) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.SelectedCell?.Position ?? 0);
        }

        private bool CanMoveToCellMeasuredCommand()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitOnElevator &&
                   !(this.SelectedCell is null) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.SelectedCell?.Position ?? 0);
        }

        private bool CanMoveToConfirm()
        {
            return this.CanBaseExecute() &&
                   this.Displacement != null;
        }

        private bool CanMoveToStartPosition()
        {
            return this.CanBaseExecute() &&
                   string.IsNullOrEmpty(this.Error) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.StartPosition) &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanToCellPositioning()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private async Task DisplacementCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveVerticalOfDistanceAsync(this.StepValue);

                if (this.Displacement is null)
                {
                    this.Displacement = Convert.ToDecimal(this.StepValue);
                }
                else
                {
                    this.Displacement += Convert.ToDecimal(this.StepValue);
                }
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

        private async Task RetrieveVerticalOffset()
        {
            try
            {
                this.CurrentVerticalOffset = await this.machineElevatorWebService.GetVerticalOffsetAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStepSinglePage(true, false);
            this.ShowNextStepSinglePage(true, true);
            this.ShowAbortStep(true, true);
        }

        private async Task StartAsync(double position)
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveManualToVerticalPositionAsync(
                    position, false, false);
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

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineService.StopMovingByAllAsync();
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

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case VerticalOffsetCalibrationStep.Start:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToCellPositioningCommand?.CanExecute() ?? false);
                    break;

                case VerticalOffsetCalibrationStep.CellMeasured:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToConfirmCommand?.CanExecute() ?? false);
                    break;

                case VerticalOffsetCalibrationStep.Confirm:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);
        }

        #endregion
    }
}
