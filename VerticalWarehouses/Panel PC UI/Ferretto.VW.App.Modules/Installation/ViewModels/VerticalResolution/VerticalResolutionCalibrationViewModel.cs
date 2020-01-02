using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
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

        private decimal? currentResolution;

        private CalibrationStep currentStep;

        private double? inputInitialPosition;

        private bool isExecutingProcedure;

        //private bool isOperationCompleted;

        private LoadingUnit loadingUnitOnBoard;

        private bool? luPresentInMachineSide;

        private bool? luPresentInOperatorSide;

        private DelegateCommand moveToBayPositionCommand;

        private ActionPolicy moveToBayPositionPolicy;

        private DelegateCommand moveToFirstMisurationCommand;

        private int positionBayId;

        //private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken sensorsToken;

        //private DelegateCommand startCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IMachineElevatorService machineElevatorService)
          : base(PresentationMode.Installer)
        {
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
        }

        #endregion

        #region Properties

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

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputInitialPosition)]);

        public bool HasStepConfirm => this.currentStep is CalibrationStep.Confirm;

        public bool HasStepFirstMisuration => this.currentStep is CalibrationStep.FirstMisuration;

        public bool HasStepLastMisuration => this.currentStep is CalibrationStep.LastMisuration;

        public bool HasStepPositionMeter => this.currentStep is CalibrationStep.PositionMeter;

        public double? InputInitialPosition
        {
            get => this.inputInitialPosition;
            set => this.SetProperty(ref this.inputInitialPosition, value, this.RaiseCanExecuteChanged);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        //public bool IsLoadingUnitOnBoard
        //{
        //    get => this.isLoadingUnitOnBoard;
        //    private set => this.SetProperty(ref this.isLoadingUnitOnBoard, value);
        //}

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        //public IMachineElevatorWebService MachineElevatorWebService { get; }

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand(
                async () => await this.MoveToBayPositionAsync(),
                this.CanMoveToBayPosition));

        public ICommand MoveToFirstMisurationCommand =>
            this.moveToFirstMisurationCommand
            ??
            (this.moveToFirstMisurationCommand = new DelegateCommand(
                () => this.CurrentStep = CalibrationStep.FirstMisuration,
                this.CanToFirstMisuration));

        //public IMachineVerticalResolutionCalibrationProcedureWebService ResolutionCalibrationService => this.resolutionCalibrationWebService;

        //public ICommand StartCommand =>
        //    this.startCommand
        //    ??
        //    (this.startCommand = new DelegateCommand(
        //        async () => await this.StartAsync(),
        //        this.CanStart));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        //internal IEventAggregator EventAggregator => this.eventAggregator;

        //internal IHealthProbeService HealthProbeService => this.healthProbeService;

        protected VerticalResolutionCalibrationProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputInitialPosition):
                        if (!this.InputInitialPosition.HasValue)
                        {
                            return $"InputInitialPosition is required.";
                        }

                        if (this.InputInitialPosition.Value <= 0)
                        {
                            return "InputInitialPosition must be strictly positive.";
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

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
        }

        public async Task GetParametersAsync()
        {
            try
            {
                this.CurrentResolution = await this.machineElevatorWebService.GetVerticalResolutionAsync();

                this.InputInitialPosition = this.ProcedureParameters.InitialPosition;
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

            // Policy
            Task.Run(async () =>
            {
                this.moveToBayPositionPolicy = await this.machineElevatorWebService.CanMoveToBayPositionAsync(this.positionBayId);

                this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
                this.moveToFirstMisurationCommand?.RaiseCanExecuteChanged();

                this.UpdateStatusButtonFooter();
            });

            this.RaisePropertyChanged(nameof(this.HasStepPositionMeter));
            this.RaisePropertyChanged(nameof(this.HasStepFirstMisuration));
            this.RaisePropertyChanged(nameof(this.HasStepLastMisuration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));

            this.RaisePropertyChanged(nameof(this.IsMoving));

            this.stopCommand?.RaiseCanExecuteChanged();
            //this.startCommand?.RaiseCanExecuteChanged();
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

        private bool CanMoveToBayPosition()
        {
            return this.CanBaseExecute() &&
                   this.moveToBayPositionPolicy?.IsAllowed == true &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanStart()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
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
                   this.moveToBayPositionPolicy?.IsAllowed == false &&
                   this.moveToBayPositionPolicy?.ReasonType == ReasonType.ElevatorInPosition &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
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

        private async Task MoveToBayPositionAsync()
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
                    this.ShowPrevStepSinglePage(false, false);
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
                    this.ShowNextStepSinglePage(false, false);
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
        //private async Task StartAsync()
        //{
        //    try
        //    {
        //        this.IsWaitingForResponse = true;
        //        this.IsExecutingProcedure = true;

        //        await this.machineElevatorWebService.MoveManualToVerticalPositionAsync(
        //            this.InputInitialPosition.Value,
        //            false,
        //            false);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.IsExecutingProcedure = false;
        //        this.ShowNotification(ex);
        //    }
        //    finally
        //    {
        //        this.IsWaitingForResponse = false;
        //    }
        //}
    }
}
