using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using ReasonType = Ferretto.VW.MAS.AutomationService.Contracts.ReasonType;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum CalibrationStep
    {
        PositionMeter,

        FirstMisuration,

        LastMisuration,

        Confirm,
    }

    [Warning(WarningsArea.Installation)]
    public class VerticalResolutionCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        //private readonly IHealthProbeService healthProbeService;

        #region Fields

        private readonly bool isLoadingUnitOnBoard;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private double axisLowerBound;

        private double axisUpperBound;

        private decimal? currentResolution;

        private CalibrationStep currentStep;

        private double? inputInitialPosition;

        private double? inputInitialPosition2;

        private bool isExecutingProcedure;

        //private bool isOperationCompleted;

        private LoadingUnit loadingUnitOnBoard;

        private bool? luPresentInMachineSide;

        private bool? luPresentInOperatorSide;

        private DelegateCommand moveToFirstMisurationCommand;

        private DelegateCommand moveToStartDestination1Command;

        private DelegateCommand moveToStartDestination2Command;

        private DelegateCommand moveToStartPositionCommand;

        private int positionBayId;

        private SubscriptionToken sensorsToken;

        private double? startPosition = 500;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IMachineElevatorService machineElevatorService)
          : base(PresentationMode.Installer)
        {
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
        }

        #endregion

        #region Properties

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

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public CalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.RaiseCanExecuteChanged);
        }

        public double? DestinationPosition1
        {
            get => this.inputInitialPosition;
            set => this.SetProperty(ref this.inputInitialPosition, value, this.RaiseCanExecuteChanged);
        }

        public double? DestinationPosition2
        {
            get => this.inputInitialPosition;
            set => this.SetProperty(ref this.inputInitialPosition2, value, this.RaiseCanExecuteChanged);
        }

        public string Error => string.Join(
            this[nameof(this.DestinationPosition1)],
            this[nameof(this.DestinationPosition2)],
            this[nameof(this.StartPosition)]);

        public bool HasStepConfirm => this.currentStep is CalibrationStep.Confirm;

        public bool HasStepFirstMisuration => this.currentStep is CalibrationStep.FirstMisuration;

        public bool HasStepLastMisuration => this.currentStep is CalibrationStep.LastMisuration;

        public bool HasStepPositionMeter => this.currentStep is CalibrationStep.PositionMeter;

        public bool IsCanStartPosition =>
            this.CanBaseExecute() &&
            !this.SensorsService.IsLoadingUnitOnElevator;

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        //public bool IsLoadingUnitOnBoard
        //{
        //    get => this.isLoadingUnitOnBoard;
        //    private set => this.SetProperty(ref this.isLoadingUnitOnBoard, value);
        //}
        //public IMachineElevatorWebService MachineElevatorWebService { get; }
        public ICommand MoveToFirstMisurationCommand =>
            this.moveToFirstMisurationCommand
            ??
            (this.moveToFirstMisurationCommand = new DelegateCommand(
                () => this.CurrentStep = CalibrationStep.FirstMisuration,
                this.CanToFirstMisuration));

        public ICommand MoveToStartDestination1Command =>
            this.moveToStartDestination1Command
            ??
            (this.moveToStartDestination1Command = new DelegateCommand(
                async () => await this.StartAsync(this.DestinationPosition1.Value),
                this.CanStart));

        public ICommand MoveToStartDestination2Command =>
            this.moveToStartDestination2Command
            ??
            (this.moveToStartDestination2Command = new DelegateCommand(
                async () => await this.StartAsync(this.DestinationPosition2.Value),
                this.CanStart));

        public ICommand MoveToStartPositionCommand =>
            this.moveToStartPositionCommand
            ??
            (this.moveToStartPositionCommand = new DelegateCommand(
                async () => await this.StartAsync(this.StartPosition.Value),
                this.CanMoveToStartPosition));

        public double? StartPosition
        {
            get => this.startPosition;
            set => this.SetProperty(ref this.startPosition, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        protected VerticalResolutionCalibrationProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.DestinationPosition1):
                        if (this.CurrentStep == CalibrationStep.FirstMisuration)
                        {
                            if (!this.DestinationPosition1.HasValue)
                            {
                                var error = $"Destination position is required.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if (this.DestinationPosition1.Value < 0)
                            {
                                var error = "Destination position must be strictly positive.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if (this.DestinationPosition1.Value < this.axisLowerBound ||
                                this.DestinationPosition1.Value > this.axisUpperBound)
                            {
                                var error = $"Destination position out of ranhe axis ({this.AxisLowerBound} - {this.AxisUpperBound}).";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }
                        }

                        break;

                    case nameof(this.StartPosition):
                        if (this.CurrentStep == CalibrationStep.PositionMeter)
                        {
                            if (!this.StartPosition.HasValue)
                            {
                                var error = $"Start position is required.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if (this.StartPosition.Value < 0)
                            {
                                var error = $"Start position must be strictly positive.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if ((this.StartPosition.Value < this.axisLowerBound ||
                                this.StartPosition.Value > this.axisUpperBound) &&
                                this.axisLowerBound > 0 &&
                                this.axisUpperBound > 0)
                            {
                                var error = $"Start position out of ranhe axis ({this.AxisLowerBound} - {this.AxisUpperBound}).";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }
                        }

                        break;

                    case nameof(this.DestinationPosition2):
                        if (this.CurrentStep == CalibrationStep.LastMisuration)
                        {
                            if (!this.DestinationPosition2.HasValue)
                            {
                                var error = $"Destination position is required.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if (this.DestinationPosition2.Value < 0)
                            {
                                var error = "Destination position must be strictly positive.";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }

                            if (this.DestinationPosition2.Value < this.axisLowerBound ||
                                this.DestinationPosition2.Value > this.axisUpperBound)
                            {
                                var error = $"Destination position out of ranhe axis ({this.AxisLowerBound} - {this.AxisUpperBound}).";
                                this.ShowNotification(error, NotificationSeverity.Warning);
                                return error;
                            }
                        }

                        break;
                }

                this.ClearNotifications();
                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
        }

        public async Task GetParametersAsync()
        {
            try
            {
                this.CurrentResolution = await this.machineElevatorWebService.GetVerticalResolutionAsync();

                this.DestinationPosition1 = this.ProcedureParameters.InitialPosition;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = CalibrationStep.PositionMeter;

            var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();
            this.AxisUpperBound = procedureParameters.UpperBound;
            this.AxisLowerBound = procedureParameters.LowerBound;

            this.positionBayId = this.MachineService.Bay.Positions.Single(p => p.Height == this.MachineService.Bay.Positions.Max(pos => pos.Height)).Id;

            // -------------------------
            await this.RetrieveProcedureParametersAsync();

            await this.GetParametersAsync();

            //this.positioningOperationChangedToken = this.positioningOperationChangedToken
            //    ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<PositioningMessageData>>()
            //        .Subscribe(
            //            this.OnPositioningOperationChanged,
            //            ThreadOption.UIThread,
            //            false);

            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        async (m) => await this.OnStepChangedAsync(m),
                        ThreadOption.UIThread,
                        false);

            this.sensorsToken = this.sensorsToken ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        async (m) => await this.OnSensorsChangedAsync(m),
                        ThreadOption.UIThread,
                        false,
                        (m) =>
                        {
                            return !this.luPresentInOperatorSide.HasValue ||
                                   !this.luPresentInMachineSide.HasValue ||
                                   (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInOperatorSide] != this.luPresentInOperatorSide.Value) ||
                                   (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInMachineSide] != this.luPresentInMachineSide.Value);
                        });

            this.IsBackNavigationAllowed = false;
        }

        //protected void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        //{
        //    if (message.IsErrored())
        //    {
        //        this.ShowNotification(
        //            VW.App.Resources.InstallationApp.ProcedureWasStopped,
        //            Services.Models.NotificationSeverity.Warning);
        //    }

        //    if (message.Data?.AxisMovement != CommonUtils.Messages.Enumerations.Axis.Vertical)
        //    {
        //        return;
        //    }

        //    if (message.IsNotRunning())
        //    {
        //        this.IsExecutingProcedure = false;
        //    }

        //    if (message.Status == MessageStatus.OperationEnd)
        //    {
        //        this.isOperationCompleted = true;
        //        this.NavigateToNextStep();
        //    }
        //}

        protected Task OnStepChangedAsync(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case CalibrationStep.PositionMeter:
                    if (e.Next)
                    {
                        this.CurrentStep = CalibrationStep.FirstMisuration;
                    }

                    break;

                case CalibrationStep.FirstMisuration:
                    if (e.Next)
                    {
                        this.CurrentStep = CalibrationStep.LastMisuration;
                    }
                    else
                    {
                        this.CurrentStep = CalibrationStep.PositionMeter;
                    }

                    break;

                case CalibrationStep.LastMisuration:
                    if (e.Next)
                    {
                        this.CurrentStep = CalibrationStep.Confirm;
                    }
                    else
                    {
                        this.CurrentStep = CalibrationStep.FirstMisuration;
                    }

                    break;

                case CalibrationStep.Confirm:
                    if (!e.Next)
                    {
                        this.CurrentStep = CalibrationStep.LastMisuration;
                    }

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        protected override void RaiseCanExecuteChanged()
        {
            if (!this.IsVisible)
            {
                return;
            }

            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.HasStepPositionMeter));
            this.RaisePropertyChanged(nameof(this.HasStepFirstMisuration));
            this.RaisePropertyChanged(nameof(this.HasStepLastMisuration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));

            this.RaisePropertyChanged(nameof(this.IsMoving));
            this.RaisePropertyChanged(nameof(this.IsCanStartPosition));

            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToStartPositionCommand?.RaiseCanExecuteChanged();

            this.moveToStartDestination1Command?.RaiseCanExecuteChanged();
            this.moveToStartDestination2Command?.RaiseCanExecuteChanged();

            this.moveToFirstMisurationCommand?.RaiseCanExecuteChanged();

            this.UpdateStatusButtonFooter();
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

        private bool CanMoveToStartPosition()
        {
            return this.CanBaseExecute() &&
                   string.IsNullOrEmpty(this.Error) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.StartPosition.Value) &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanStart()
        {
            return
                this.CanBaseExecute() &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanToFirstMisuration()
        {
            return this.CanBaseExecute() &&
                   !this.SensorsService.IsLoadingUnitOnElevator &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.StartPosition.Value);
        }

        private async Task GetSensorsAndLoadingUnitOnBoardAsync()
        {
            if (this.HealthProbeService.HealthStatus != HealthStatus.Healthy)
            {
                return;
            }

            try
            {
                this.loadingUnitOnBoard = await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync();
                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                var sensors = new Sensors();
                sensors.Update(sensorsStates.ToArray());

                //this.SetIsLoadingUnitOnBord(sensors);
            }
            catch
            {
                throw;
            }
        }

        private async Task MoveToStartPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.positionBayId,
                    computeElongation: true,
                    performWeighting: true);

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

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.luPresentInOperatorSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
            this.luPresentInMachineSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
        }

        //        this.NavigationService.Appear(
        //            nameof(Utils.Modules.Installation),
        //            Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION,
        //            wizardData,
        //            trackCurrentView: false);
        //    }
        //}
        private async Task RetrieveProcedureParametersAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ProcedureParameters = await this.resolutionCalibrationWebService.GetParametersAsync();
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

        private void ShowSteps()
        {
            this.ShowPrevStepSinglePage(false, false);
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
                case CalibrationStep.PositionMeter:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToFirstMisurationCommand?.CanExecute() ?? false);
                    break;

                case CalibrationStep.FirstMisuration:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CalibrationStep.LastMisuration:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CalibrationStep.Confirm:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }
        }

        #endregion

        //private void NavigateToNextStep()
        //{
        //    if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION))
        //    {
        //        var wizardData = new VerticalResolutionWizardData
        //        {
        //            CurrentResolution = this.CurrentResolution.Value,
        //            FinalPosition = this.ProcedureParameters.FinalPosition,
        //            InitialPosition = this.InputInitialPosition.Value,
        //        };
        //private void SetIsLoadingUnitOnBord(Sensors sensors)
        //{
        //    this.IsLoadingUnitOnBoard = sensors.LuPresentInMachineSide || sensors.LuPresentInOperatorSide;
        //}
    }
}
