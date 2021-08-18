using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum VerticalOffsetCalibrationStep
    {
        CellMeasured,

        Confirm,

        OriginCalibration
    }

    [Warning(WarningsArea.Installation)]
    public class VerticalOffsetCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

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

        private bool isOriginCalibrationStepVisible;

        private bool missionInError;

        private DelegateCommand moveToCellCommand;

        private DelegateCommand moveToCellMeasuredCommand;

        private DelegateCommand moveToCellPositioningCommand;

        private DelegateCommand moveToConfirmCommand;

        private DelegateCommand moveToStartPositionCommand;

        private double newDisplacement;

        private SubscriptionToken receiveHomingUpdateToken;

        private DelegateCommand reloadCommand;

        private Cell selectedCell;

        private DelegateCommand startOriginCalibrationCommand;

        private double startPosition;

        private SubscriptionToken stepChangedToken;

        private double stepValue;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(
            IMachineErrorsWebService machineErrorsWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineVerticalOffsetProcedureWebService verticalOffsetWebService,
            IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.verticalOffsetWebService = verticalOffsetWebService ?? throw new ArgumentNullException(nameof(verticalOffsetWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured;
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                 async () => await this.ApplyCorrectionAsync(),
                this.CanBaseExecute));

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
                this.RaisePropertyChanged(nameof(this.Displacement));
                this.StepValue = 0;
                this.UpdateSelectedCell();
            });
        }

        public VerticalOffsetCalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
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

        public bool HasStepOriginCalibration => this.currentStep is VerticalOffsetCalibrationStep.OriginCalibration;

        public bool IsCanStartPosition => this.CanBaseExecute();

        public bool IsCanStepValue => this.CanBaseExecute();

        public bool IsOriginCalibrationStepVisible
        {
            get => this.isOriginCalibrationStepVisible;
            set => this.SetProperty(ref this.isOriginCalibrationStepVisible, value);
        }

        public ICommand MoveToCellCommand =>
                    this.moveToCellCommand
            ??
            (this.moveToCellCommand = new DelegateCommand(
                async () =>
                {
                    await this.StartAsync(this.SelectedCell.Position);
                },
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
                    if (this.Displacement != null)
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.Confirm;
                        this.NewDisplacement = this.CurrentVerticalOffset.Value - (double)this.Displacement.Value;

                        this.IsOriginCalibrationStepVisible = (this.Displacement.Value != 0);
                        this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));
                    }
                    else
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.OriginCalibration;

                        this.IsOriginCalibrationStepVisible = true;
                        this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));
                    }
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
                () => this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured,
                this.CanBaseExecute));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            protected set => this.SetProperty(ref this.selectedCell, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StartOriginCalibrationCommand =>
         this.startOriginCalibrationCommand
          ??
          (this.startOriginCalibrationCommand = new DelegateCommand(
              async () => await this.StartOriginCalibrationAsync(),
              this.CanExecuteStartOriginCalibrationCommand));

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
                this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.CurrentCellId):
                        if (this.CurrentStep == VerticalOffsetCalibrationStep.CellMeasured &&
                            !this.IsMoving)
                        {
                            if (!this.CurrentCellId.HasValue)
                            {
                                this.currentError = Localized.Get("InstallationApp.CellSelectedRequired");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if (this.SelectedCell?.BlockLevel != BlockLevel.None)
                            {
                                this.currentError = Localized.Get("InstallationApp.CellSelectedBlocked");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
                            else if (this.SelectedCell is null || this.CurrentCellId is null)
                            {
                                this.currentError = Localized.Get("InstallationApp.CellSelectedNotPresent");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
                        }

                        break;

                    case nameof(this.StepValue):
                        if (this.CurrentStep == VerticalOffsetCalibrationStep.CellMeasured &&
                            !this.IsMoving)
                        {
                        }

                        break;
                }

                if (this.IsVisible && string.IsNullOrEmpty(this.currentError))
                {
                    //this.ClearNotifications();
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }

            if (this.receiveHomingUpdateToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<HomingMessageData>>().Unsubscribe(this.receiveHomingUpdateToken);
                this.receiveHomingUpdateToken?.Dispose();
                this.receiveHomingUpdateToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            var newMissions = await this.machineMissionsWebService.GetAllAsync();
            var errors = await this.machineErrorsWebService.GetAllAsync();

            if (newMissions.Any(s => s.Step >= MAS.AutomationService.Contracts.MissionStep.Error))
            {
                this.missionInError = true;
                this.ClearNotifications();
                this.ShowNotification(
                     Localized.Get("ServiceMachine.MissionInError"),
                     NotificationSeverity.Error);
            }
            else if (errors.Any(s => s.ResolutionDate == null))
            {
                this.missionInError = true;
                this.ClearNotifications();
                this.ShowNotification(
                     Localized.Get("InstallationApp.ErrorActiveWarning"),
                     NotificationSeverity.Error);
            }
            else
            {
                this.missionInError = false;
            }

            this.moveToCellPositioningCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();

            this.UpdateSelectedCell();

            if (this.CurrentStep != VerticalOffsetCalibrationStep.OriginCalibration)
            {
                this.IsOriginCalibrationStepVisible = false;
                this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));
            }

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                if (!this.CurrentVerticalOffset.HasValue || this.AxisUpperBound == 0 || this.AxisLowerBound == 0 || this.StartPosition == 0)
                {
                    await this.RetrieveVerticalOffset();
                    await this.GetAxisBound();

                    var procedureCalibrationParameters = await this.resolutionCalibrationWebService.GetParametersAsync();
                    this.StartPosition = procedureCalibrationParameters.StartPosition;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case VerticalOffsetCalibrationStep.CellMeasured:
                    if (e.Next)
                    {
                        this.CurrentStep = VerticalOffsetCalibrationStep.Confirm;
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
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsCanStartPosition));
            this.RaisePropertyChanged(nameof(this.IsCanStepValue));
            this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));

            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToStartPositionCommand?.RaiseCanExecuteChanged();
            this.moveToCellPositioningCommand?.RaiseCanExecuteChanged();
            this.moveToCellCommand?.RaiseCanExecuteChanged();
            this.moveToCellMeasuredCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmCommand?.RaiseCanExecuteChanged();
            this.displacementCommand?.RaiseCanExecuteChanged();
            this.reloadCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.startOriginCalibrationCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.AxisLowerBound > this.NewDisplacement)
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ModifyLowerBoundDialog"), Localized.Get("InstallationApp.LowPositionControl"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.No)
                    {
                        return;
                    }

                    await this.machineElevatorWebService.UpdateVerticalLowerBoundAsync(this.NewDisplacement);

                    await this.GetAxisBound();
                }

                if ((this.Displacement.HasValue) && (this.Displacement.Value == 0))
                {
                    await this.verticalOffsetWebService.UpdateVerticalOffsetAndCompleteAsync(this.NewDisplacement);

                    await this.RetrieveVerticalOffset();

                    this.Displacement = null;

                    this.ShowNotification(Localized.Get("InstallationApp.AxisVerticalOffsetUpdated"), Services.Models.NotificationSeverity.Success);

                    this.NavigationService.GoBack();
                }
                else
                {
                    await this.verticalOffsetWebService.UpdateVerticalOffsetAsync(this.NewDisplacement);

                    await this.RetrieveVerticalOffset();

                    this.Displacement = null;

                    this.ShowNotification(Localized.Get("InstallationApp.AxisVerticalOffsetUpdated"), Services.Models.NotificationSeverity.Success);

                    this.CurrentStep = VerticalOffsetCalibrationStep.OriginCalibration;
                }
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

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanDisplacementCommand()
        {
            return this.CanBaseExecute() &&
                   this.StepValue != 0;
        }

        private bool CanExecuteStartOriginCalibrationCommand()
        {
            return !this.MachineService.MachineStatus.IsMoving &&
                   !this.MachineService.MachineStatus.IsMovingLoadingUnit &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanMoveToCellCommand()
        {
            return this.CanBaseExecute() &&
                   this.SelectedCell?.BlockLevel == BlockLevel.None &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.SelectedCell?.Position ?? 0);
        }

        private bool CanMoveToCellMeasuredCommand()
        {
            return this.CanBaseExecute() &&
                   !(this.SelectedCell is null) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.SelectedCell?.Position ?? 0);
        }

        private bool CanMoveToConfirm()
        {
            return this.CanBaseExecute();
            //&&
            //   this.Displacement != null;
        }

        private bool CanMoveToStartPosition()
        {
            return this.CanBaseExecute() &&
                   string.IsNullOrEmpty(this.Error) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.StartPosition);
        }

        private bool CanStop()
        {
            return this.IsMoving &&
                   !this.IsWaitingForResponse;
        }

        private bool CanToCellPositioning()
        {
            return this.CanBaseExecute() &&
                !this.missionInError;
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
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task GetAxisBound()
        {
            var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();
            this.AxisUpperBound = procedureParameters.UpperBound;
            this.AxisLowerBound = procedureParameters.LowerBound;
        }

        private async Task OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            if (message.Data.AxisToCalibrate == Axis.HorizontalAndVertical)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingStarted"));

                        break;

                    case MessageStatus.OperationEnd:

                        await this.verticalOffsetWebService.CompleteProcedureAsync();

                        this.CurrentStep = VerticalOffsetCalibrationStep.CellMeasured;

                        this.ShowNotification(Localized.Get("InstallationApp.ProcedureCompleted"), Services.Models.NotificationSeverity.Success);

                        this.NavigationService.GoBack();

                        break;

                    case MessageStatus.OperationError:
                        this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingError"),
                            Services.Models.NotificationSeverity.Error);

                        break;
                }
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

        private async Task StartAsync(double position)
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveManualToVerticalPositionAsync(
                    position, false, false, null);
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

        private async Task StartOriginCalibrationAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.verticalOriginProcedureWebService.StartAsync();
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

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.receiveHomingUpdateToken = this.receiveHomingUpdateToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        async (m) =>
                        {
                            if (this.currentStep == VerticalOffsetCalibrationStep.OriginCalibration)
                            { await this.OnHomingProcedureStatusChanged(m); }
                        },
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepCellMeasured));
                           this.RaisePropertyChanged(nameof(this.HasStepConfirm));
                           this.RaisePropertyChanged(nameof(this.HasStepOriginCalibration));
                           this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateSelectedCell()
        {
            if (this.CurrentCellId is null)
            {
                this.SelectedCell = this.MachineService.Cells.FirstOrDefault(x => x.BlockLevel != BlockLevel.Blocked);
                this.CurrentCellId = this.SelectedCell?.Id;
            }
            else
            {
                this.SelectedCell = this.MachineService.Cells.SingleOrDefault(c => c.Id.Equals(this.CurrentCellId));
            }
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case VerticalOffsetCalibrationStep.CellMeasured:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToConfirmCommand?.CanExecute() ?? false);
                    break;

                case VerticalOffsetCalibrationStep.Confirm:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case VerticalOffsetCalibrationStep.OriginCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepCellMeasured));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
            this.RaisePropertyChanged(nameof(this.HasStepOriginCalibration));
            this.RaisePropertyChanged(nameof(this.IsOriginCalibrationStepVisible));
        }

        #endregion
    }
}
