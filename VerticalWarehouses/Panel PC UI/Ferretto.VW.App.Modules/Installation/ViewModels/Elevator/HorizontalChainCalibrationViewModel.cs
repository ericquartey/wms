using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum HorizontalChainCalibrationStep
    {
        ChainCalibration,

        GoToBay,

        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment,
    }

    [Warning(WarningsArea.Installation)]
    public class HorizontalChainCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private DelegateCommand closedShutterCommand;

        private DelegateCommand completeCommand;

        private DelegateCommand confirmCalibration;

        private double? currentDistance;

        private double? currentResolution;

        private HorizontalChainCalibrationStep currentStep;

        private double? cyclesPercent;

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isChainOffsetVisible;

        private bool isElevatorMovingToBay;

        private bool isExecutingChainCalibration;

        private bool isExecutingProcedure;

        private bool isExecutingStopInPhase;

        private bool isNewErrorValueVisible;

        private bool isShutterMoving;

        private bool isTuningChain;

        private bool isUseWeightControl;

        private double? measured;

        private double? measuredDistance;

        private DelegateCommand moveToBayPositionCommand;

        private DelegateCommand moveToGoToBayCommand;

        private DelegateCommand moveToStartCalibrationCommand;

        private int? newErrorValue;

        private int performedCycles;

        private SubscriptionToken profileCalibrationToken;

        private int requiredCycles;

        private DelegateCommand returnCalibration;

        private int sessionPerformedCycles;

        private DelegateCommand startCalibrationCommand;

        private int startPerformedCycles;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopInPhaseCommand;

        private SubscriptionToken themeChangedToken;

        private DelegateCommand tuningChainCommand;

        #endregion

        #region Constructors

        public HorizontalChainCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
          : base(PresentationMode.Installer)
        {
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.CurrentStep = HorizontalChainCalibrationStep.ChainCalibration;
        }

        #endregion

        #region Properties

        public bool BayIsShutterThreeSensors => this.MachineService.IsShutterThreeSensors;

        public double? ChainOffset => Math.Abs(this.MachineService.Bay.ChainOffset);

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public ICommand CompleteCommand =>
                            this.completeCommand
            ??
            (this.completeCommand = new DelegateCommand(
                async () => await this.CompleteAsync(), this.CanComplete));

        public ICommand ConfirmCalibration =>
            this.confirmCalibration
            ??
            (this.confirmCalibration = new DelegateCommand(
                async () => await this.ConfirmCalibrationAsync()));

        public double? CurrentDistance
        {
            get => this.currentDistance;
            protected set => this.SetProperty(ref this.currentDistance, value);
        }

        public double? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public HorizontalChainCalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, () => this.UpdateStatusButtonFooter(false));
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error => string.Join(
            this[nameof(this.CurrentDistance)],
            this[nameof(this.CurrentResolution)],
            this[nameof(this.NewErrorValue)]);

        public bool HasBayExternal => this.MachineService.HasBayExternal;

        public bool HasShutter => this.MachineService.HasShutter;

        public bool HasStepChainCalibration => this.currentStep is HorizontalChainCalibrationStep.ChainCalibration;

        public bool HasStepConfirmAdjustment => this.currentStep is HorizontalChainCalibrationStep.ConfirmAdjustment;

        public bool HasStepGoToBay => this.currentStep is HorizontalChainCalibrationStep.GoToBay;

        public bool HasStepRunningCalibration => this.currentStep is HorizontalChainCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is HorizontalChainCalibrationStep.StartCalibration;

        public bool IsCalibrationCompletedOrStopped
        {
            get => this.isCalibrationCompletedOrStopped;
            private set
            {
                if (this.SetProperty(ref this.isCalibrationCompletedOrStopped, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCalibrationNotCompleted
        {
            get => this.isCalibrationNotCompleted;
            set => this.SetProperty(ref this.isCalibrationNotCompleted, value);
        }

        public bool IsChainOffsetVisible
        {
            get => this.isChainOffsetVisible;
            set => this.SetProperty(ref this.isChainOffsetVisible, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set => this.SetProperty(ref this.isElevatorMovingToBay, value);
        }

        public bool IsExecutingChainCalibration
        {
            get => this.isExecutingChainCalibration;
            private set
            {
                if (this.SetProperty(ref this.isExecutingChainCalibration, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingStopInPhase
        {
            get => this.isExecutingStopInPhase;
            private set
            {
                if (this.SetProperty(ref this.isExecutingStopInPhase, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsNewErrorValueVisible
        {
            get => this.isNewErrorValueVisible;
            set => this.SetProperty(ref this.isNewErrorValueVisible, value);
        }

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set => this.SetProperty(ref this.isShutterMoving, value);
        }

        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set => this.SetProperty(ref this.isTuningChain, value);
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public double? Measured
        {
            get => this.measured;
            protected set => this.SetProperty(ref this.measured, value);
        }

        public double? MeasuredDistance
        {
            get => this.measuredDistance;
            protected set => this.SetProperty(ref this.measuredDistance, value);
        }

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand(
                async () => await this.MoveToBayPositionAsync(),
                () => this.CanMoveToBayPosition()));

        public ICommand MoveToGoToBayCommand =>
            this.moveToGoToBayCommand
            ??
            (this.moveToGoToBayCommand = new DelegateCommand(
                () => this.CurrentStep = HorizontalChainCalibrationStep.GoToBay,
                () => this.CanMoveToGoToBay()));

        public ICommand MoveToStartCalibrationCommand =>
            this.moveToStartCalibrationCommand
            ??
            (this.moveToStartCalibrationCommand = new DelegateCommand(
                () => this.CurrentStep = HorizontalChainCalibrationStep.StartCalibration,
                () => this.CanMoveToStartCalibration()));

        public int? NewErrorValue
        {
            get => this.newErrorValue;
            set => this.SetProperty(ref this.newErrorValue, value);
        }

        public int PerformedCycles
        {
            get => this.performedCycles;
            set
            {
                if (this.SetProperty(ref this.performedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                if (this.SetProperty(ref this.requiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ReturnCalibration =>
                                                                                                                                                                                                           this.returnCalibration
           ??
           (this.returnCalibration = new DelegateCommand(
               async () =>
               {
                   try
                   {
                       this.CurrentStep = HorizontalChainCalibrationStep.ChainCalibration;
                   }
                   catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                   {
                       this.ShowNotification(ex);
                   }
               }));

        public int SessionPerformedCycles
        {
            get => this.sessionPerformedCycles;
            set
            {
                if (this.SetProperty(ref this.sessionPerformedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StartCalibrationCommand =>
            this.startCalibrationCommand
            ??
            (this.startCalibrationCommand = new DelegateCommand(
               async () => await this.StartCalibrationAsync(),
               this.CanStartCalibration));

        public int StartPerformedCycles
        {
            get => this.startPerformedCycles;
            set
            {
                if (this.SetProperty(ref this.startPerformedCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        public ICommand StopInPhaseCommand =>
            this.stopInPhaseCommand
            ??
            (this.stopInPhaseCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStopInPhase));

        public ICommand TuningChainCommand =>
            this.tuningChainCommand
            ??
            (this.tuningChainCommand = new DelegateCommand(
                async () => await this.TuningChainAsync(),
                this.CanTuningChain));

        protected Carousel ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.NewErrorValue):
                        if (!this.NewErrorValue.HasValue)
                        {
                            return Localized.Get("InstallationApp.MissValue");
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue < 1)
                        {
                            return Localized.Get("InstallationApp.DataBePositive");
                        }

                        if (this.NewErrorValue.HasValue && this.NewErrorValue > 9)
                        {
                            return Localized.Get("InstallationApp.MaxValue9");
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

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken.Dispose();
                this.stepChangedToken = null;
            }

            if (this.profileCalibrationToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>().Unsubscribe(this.profileCalibrationToken);
                this.profileCalibrationToken?.Dispose();
                this.profileCalibrationToken = null;
            }

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetBayPosition()
        {
            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
            }

            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
            }

            if (this.MachineService.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter(true);

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.machineCarouselWebService.GetParametersAsync();

                this.RequiredCycles = procedureParameters.RequiredCycles;
                this.PerformedCycles = procedureParameters.PerformedCycles;
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

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.RetrieveProcedureInformationAsync();
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

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            if (!this.IsMoving)
            {
                this.IsTuningChain = false;
                this.IsExecutingChainCalibration = false;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case HorizontalChainCalibrationStep.ChainCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.GoToBay;
                    }

                    break;

                case HorizontalChainCalibrationStep.GoToBay:
                    if (e.Next)
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.StartCalibration;
                    }
                    else
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.ChainCalibration;
                    }

                    break;

                case HorizontalChainCalibrationStep.StartCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.RunningCalibration;
                    }
                    else
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.GoToBay;
                    }

                    break;

                case HorizontalChainCalibrationStep.RunningCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.ConfirmAdjustment;
                    }
                    else
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.ChainCalibration;
                    }

                    break;

                case HorizontalChainCalibrationStep.ConfirmAdjustment:
                    if (!e.Next)
                    {
                        this.CurrentStep = HorizontalChainCalibrationStep.ChainCalibration;
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

            this.stopCommand?.RaiseCanExecuteChanged();
            this.stopInPhaseCommand?.RaiseCanExecuteChanged();

            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.completeCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();
            this.moveToGoToBayCommand?.RaiseCanExecuteChanged();
            this.moveToStartCalibrationCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));
            this.RaisePropertyChanged(nameof(this.IsExecutingStopInPhase));

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanCloseShutter()
        {
            return
                this.CanBaseExecute()
                //&& !this.IsShutterMoving
                && ((this.SensorsService?.IsZeroChain ?? false) || this.SensorsService.IsLoadingUnitOnElevator)
                &&
                this.SensorsService.ShutterSensors != null
                &&
                !this.SensorsService.ShutterSensors.Closed;
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanMoveToBayPosition()
        {
            return this.CanBaseExecute();
        }

        private bool CanMoveToGoToBay()
        {
            return this.CanBaseExecute() &&
                   this.MachineStatus.ElevatorHorizontalPosition == 0;
        }

        private bool CanMoveToStartCalibration()
        {
            return this.CanBaseExecute() &&
                this.MachineStatus.ElevatorPositionType == ElevatorPositionType.Bay;
        }

        private bool CanStartCalibration()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh &&
                   (this.SensorsService.ShutterSensors.Closed || !this.HasShutter) &&
                   (this.SensorsService.BayZeroChain || !this.MachineService.HasCarousel);
        }

        private bool CanStop()
        {
            if (this.CurrentStep == HorizontalChainCalibrationStep.RunningCalibration)
            {
                return !this.IsKeyboardOpened
                        &&
                        this.MachineModeService.MachinePower == MachinePowerState.Powered;
            }
            else
            {
                return this.IsMoving;
            }
        }

        private bool CanStopInPhase()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse && !this.IsExecutingStopInPhase;
        }

        private bool CanTuningChain()
        {
            return (this.HasBayExternal || this.SensorsService.ShutterSensors.Closed) &&

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

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(MAS.AutomationService.Contracts.ShutterPosition.Closed);
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

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmCalibrationProcedure"), Localized.Get("InstallationApp.HorizontalCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsExecutingStopInPhase = false;
                    this.IsExecutingProcedure = false;

                    await this.machineCarouselWebService.SetCalibrationCompletedAsync();

                    this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                            Services.Models.NotificationSeverity.Success);
                }

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.CurrentStep = HorizontalChainCalibrationStep.StartCalibration;
                this.IsWaitingForResponse = false;
            }
        }

        private async Task ConfirmCalibrationAsync()
        {
            try
            {
                if (this.MeasuredDistance.HasValue)
                {
                    await this.machineElevatorWebService.SetHorizontalChainCalibrationDistanceAsync(this.MeasuredDistance.Value);
                }
                await this.MachineService.OnUpdateServiceAsync();
                await this.machineElevatorWebService.SetHorizontalChainCalibrationCompletedAsync();

                this.ShowNotification(
                        VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                        Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();

                this.CurrentStep = HorizontalChainCalibrationStep.StartCalibration;
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

        private async Task MoveToBayPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var bay = await this.bayManager.GetBayAsync();
                var bayPosition = bay.Positions.Single(b => b.Height == bay.Positions.Max(p => p.Height));

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    bayPosition.Id,
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
                this.IsElevatorMovingToBay = false;
            }
        }

        private void OnProfileCalibrationMessage(NotificationMessageUI<ProfileCalibrationMessageData> message)
        {
            var data = message.Data as ProfileCalibrationMessageData;

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored() ||
                this.MachineError != null)
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;

                this.IsChainOffsetVisible = true;
                this.IsNewErrorValueVisible = false;

                this.IsExecutingProcedure = false;

                this.IsCalibrationNotCompleted = false;

                this.CurrentStep = HorizontalChainCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
                    this.IsChainOffsetVisible = false;
                    this.IsNewErrorValueVisible = false;
                    this.IsCalibrationNotCompleted = true;
                }

                return;
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);

                this.IsCalibrationNotCompleted = false;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = true;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;

                this.IsCalibrationCompletedOrStopped = true;

                this.MeasuredDistance = data.Measured;

                this.NewErrorValue = 0;
                this.CurrentStep = HorizontalChainCalibrationStep.ConfirmAdjustment;

                this.Measured = message.Data.Measured;

                this.RaiseCanExecuteChanged();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationNotCompleted = true;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = false;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = 0;

                this.CurrentStep = HorizontalChainCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();

                this.IsExecutingProcedure = false;
            }
        }

        private async Task StartCalibrationAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    await this.machineElevatorWebService.MoveHorizontalCalibrationAsync(MAS.AutomationService.Contracts.HorizontalMovementDirection.Forwards);

                    this.CurrentStep = HorizontalChainCalibrationStep.RunningCalibration;
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
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;

                if (this.currentStep == HorizontalChainCalibrationStep.RunningCalibration)
                {
                    this.CurrentStep = HorizontalChainCalibrationStep.StartCalibration;
                }
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

            this.profileCalibrationToken = this.profileCalibrationToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<ProfileCalibrationMessageData>>()
                    .Subscribe(
                        (m) => this.OnProfileCalibrationMessage(m),
                        ThreadOption.UIThread,
                        false);

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepChainCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepGoToBay));
                           this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
                       },
                       ThreadOption.UIThread,
                       false);
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
                    this.IsExecutingChainCalibration = true;
                }
                catch (Exception ex)
                {
                    this.IsTuningChain = false;
                    this.IsExecutingChainCalibration = false;

                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsExecutingProcedure = false;
                    this.IsWaitingForResponse = false;
                }
            }
        }

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case HorizontalChainCalibrationStep.ChainCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToGoToBayCommand?.CanExecute() ?? false);
                    break;

                case HorizontalChainCalibrationStep.GoToBay:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, this.moveToStartCalibrationCommand?.CanExecute() ?? false);
                    break;

                case HorizontalChainCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case HorizontalChainCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case HorizontalChainCalibrationStep.ConfirmAdjustment:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepChainCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepGoToBay));
            this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
        }

        #endregion
    }
}
