using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum CarouselCalibrationStep
    {
        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment
    }

    [Warning(WarningsArea.Installation)]
    public class CarouselCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand applyCommand;

        private DelegateCommand completeCommand;

        private double? currentDistance;

        private double? currentResolution;

        private CarouselCalibrationStep currentStep;

        private double? cyclesPercent;

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isExecutingProcedure;

        private bool isExecutingStopInPhase;

        private bool isTuningBay;

        private DateTime lastTime = DateTime.Now;

        private double? newErrorValue;

        private int oldPerformedCycle = 0;

        private int performedCycles;

        private SubscriptionToken positioningMessageReceivedToken;

        private TimeSpan remainingTime = new TimeSpan();

        private DelegateCommand repeatCalibrationCommand;

        private int requiredCycles;

        private int sessionPerformedCycles;

        private DelegateCommand startCalibrationCommand;

        private int startPerformedCycles;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopInPhaseCommand;

        private DelegateCommand tuningBayCommand;

        #endregion

        #region Constructors

        public CarouselCalibrationViewModel(IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineBaysWebService machineBaysWebService
            )
          : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

            this.CurrentStep = CarouselCalibrationStep.StartCalibration;
        }

        #endregion

        #region Properties

        public ICommand ApplyCommand =>
            this.applyCommand
            ??
            (this.applyCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(), this.CanApply));

        public ICommand CompleteCommand =>
            this.completeCommand
            ??
            (this.completeCommand = new DelegateCommand(
                async () => await this.CompleteAsync(), this.CanComplete));

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

        public CarouselCalibrationStep CurrentStep
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

        public bool HasStepConfirmAdjustment => this.currentStep is CarouselCalibrationStep.ConfirmAdjustment;

        public bool HasStepRunningCalibration => this.currentStep is CarouselCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is CarouselCalibrationStep.StartCalibration;

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

        public bool IsErrorNegative
        {
            get => this.isErrorNegative;
            set
            {
                if (this.SetProperty(ref this.isErrorNegative, value))
                {
                    this.isErrorPositive = !this.isErrorNegative;
                    this.RaisePropertyChanged(nameof(this.IsErrorPositive));
                }
            }
        }

        public bool IsErrorPositive
        {
            get => this.isErrorPositive;
            set
            {
                if (this.SetProperty(ref this.isErrorPositive, value))
                {
                    this.isErrorNegative = !this.isErrorPositive;
                    this.RaisePropertyChanged(nameof(this.IsErrorNegative));
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

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set => this.SetProperty(ref this.isTuningBay, value);
        }

        public double? NewErrorValue
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

        public TimeSpan RemainingTime
        {
            get => new TimeSpan(this.remainingTime.Hours, this.remainingTime.Minutes, this.remainingTime.Seconds);
            set => this.SetProperty(ref this.remainingTime, value, this.RaiseCanExecuteChanged);
        }

        public ICommand RepeatCalibrationCommand =>
            this.repeatCalibrationCommand
            ??
            (this.repeatCalibrationCommand = new DelegateCommand(
               () =>
               {
                   this.CurrentStep = CarouselCalibrationStep.StartCalibration;
               },
                this.CanRepeat));

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
               () =>
               this.StartCalibrationAsync(),
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
                () =>
                {
                    this.StopInPhaseAsync();
                },
                this.CanStopInPhase));

        public ICommand TuningBayCommand =>
            this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuneBayAsync(),
                this.CanTuneBay));

        protected Carousel ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                //this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.NewErrorValue):
                        if (!this.NewErrorValue.HasValue)
                        {
                            return "manca il valore";
                        }

                        break;
                }

                if (this.IsVisible /*&& string.IsNullOrEmpty(this.currentError)*/)
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
                this.stepChangedToken.Dispose();
                this.stepChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter(true);

            await this.RetrieveProcedureInformationAsync();

            this.RaiseCanExecuteChanged();

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
                await this.SensorsService.RefreshAsync(true);

                await this.RetrieveProcedureInformationAsync();

                // devo controllare che non sia cambiata dai parametri o altre baie
                this.CurrentResolution = this.MachineService.Bay.Resolution;

                this.CurrentDistance = this.MachineService.Bay.Carousel.ElevatorDistance;

                this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving;

                this.RaiseCanExecuteChanged();
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
                case CarouselCalibrationStep.StartCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = CarouselCalibrationStep.RunningCalibration;
                    }

                    break;

                case CarouselCalibrationStep.RunningCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                    }
                    else
                    {
                        this.CurrentStep = CarouselCalibrationStep.StartCalibration;
                    }

                    break;

                case CarouselCalibrationStep.ConfirmAdjustment:
                    if (!e.Next)
                    {
                        this.CurrentStep = CarouselCalibrationStep.StartCalibration;
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

            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();
            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.applyCommand?.RaiseCanExecuteChanged();
            this.completeCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.RemainingTime));
            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));
            this.RaisePropertyChanged(nameof(this.IsExecutingStopInPhase));

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
        }

        private async Task ApplyCorrectionAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage("Vuoi davvero applicare la correzione?", "Calibrazione giostra", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    var nev = this.NewErrorValue;
                    var measuredCorrection = this.IsErrorNegative ? -this.NewErrorValue : this.NewErrorValue;

                    var correctionForEachMovement = measuredCorrection / this.SessionPerformedCycles;

                    double newElevatorDistance = (double)correctionForEachMovement + this.MachineService.Bay.Carousel.ElevatorDistance;

                    await this.machineCarouselWebService.UpdateElevatorChainDistanceAsync(newElevatorDistance);

                    await this.MachineService.OnUpdateServiceAsync();

                    this.CurrentDistance = this.MachineService.Bay.Carousel.ElevatorDistance;

                    this.ShowNotification(
                            VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
                            Services.Models.NotificationSeverity.Success);
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

        private bool CanApply()
        {
            return this.CanBaseExecute();
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanRepeat()
        {
            return this.CanBaseExecute();
        }

        private bool CanStartCalibration()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanStop()
        {
            return
                this.IsMoving;
        }

        private bool CanStopInPhase()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse && !this.IsExecutingStopInPhase;
        }

        private bool CanTuneBay()
        {
            return this.CanBaseExecute() &&
                   !this.IsTuningBay &&
                   this.MachineStatus.LoadingUnitPositionDownInBay is null &&
                   this.MachineStatus.LoadingUnitPositionUpInBay is null
                   //&&
                   //!this.SensorsService.Sensors.ACUBay1S3IND
                   ;
        }

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage("Desideri confermare la procedura di calibrazione?", "Calibrazione giostra", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsExecutingStopInPhase = false;
                    this.IsExecutingProcedure = false;

                    await this.machineCarouselWebService.SetCalibrationCompletedAsync();

                    this.ShowNotification(
                            VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
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
                this.CurrentStep = CarouselCalibrationStep.StartCalibration;
                this.IsWaitingForResponse = false;
            }
        }

        private async void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode != MovementMode.BayTest)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored() ||
                this.MachineError != null)
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = this.MachineService.Bay.ChainOffset;

                this.IsExecutingProcedure = false;

                this.IsCalibrationNotCompleted = false;

                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
                    this.IsCalibrationNotCompleted = true;
                    return;
                }
            }

            // ad ogni ciclo completato...aggiornamento dati
            if (message.Status == MessageStatus.OperationExecuting)
            {
                // update cycle info
                if (!this.IsExecutingStopInPhase)
                {
                    this.RequiredCycles = message.Data.RequiredCycles;
                }
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.UpdateRemainingTime();
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                if (message.Data.IsTestStopped)
                {
                    this.ShowNotification("Test stoppato in fase", Services.Models.NotificationSeverity.Success);
                }
                else
                {
                    this.ShowNotification(VW.App.Resources.InstallationApp.CompletedTest, Services.Models.NotificationSeverity.Success);
                }

                this.IsCalibrationNotCompleted = false;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;

                this.IsCalibrationCompletedOrStopped = true;

                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.NewErrorValue = 0;
                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationNotCompleted = true;
                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = 0;

                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();

                this.IsExecutingProcedure = false;
            }
        }

        private async Task StartCalibrationAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage("Vuoi iniziare la procedura", "Calibrazione giostra", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineCarouselWebService.ResetCalibrationAsync();

                    await this.RetrieveProcedureInformationAsync();

                    // Update procedure info
                    this.StartPerformedCycles = this.PerformedCycles;
                    this.SessionPerformedCycles = 0;

                    this.oldPerformedCycle = this.PerformedCycles;
                    this.lastTime = DateTime.Now;

                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    await this.machineCarouselWebService.StartCalibrationAsync();

                    this.CurrentStep = CarouselCalibrationStep.RunningCalibration;
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
            }
        }

        private async Task StopInPhaseAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsExecutingProcedure = false;
            this.IsExecutingStopInPhase = true;
            try
            {
                await this.machineCarouselWebService.StopCalibrationAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
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

            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
               ??
               this.eventAggregator
                   .GetEvent<NotificationEventUI<PositioningMessageData>>()
                   .Subscribe(
                       this.OnPositioningMessageReceived,
                       ThreadOption.UIThread,
                       false);
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, InstallationApp.BayCalibration, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsTuningBay = true;
                    await this.machineCarouselWebService.FindZeroAsync();

                    this.IsExecutingProcedure = true;
                }
            }
            catch (Exception ex)
            {
                this.IsTuningBay = false;
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
                this.IsTuningBay = false;
            }
        }

        private void UpdateRemainingTime()
        {
            try
            {
                if ((this.oldPerformedCycle == 0) && (this.PerformedCycles == 0))
                {
                }
                else
                {
                    if (this.PerformedCycles > this.oldPerformedCycle)
                    {
                        this.oldPerformedCycle = this.PerformedCycles;
                        var lastCycleTime = DateTime.Now - this.lastTime;
                        this.lastTime = DateTime.Now;

                        this.RemainingTime = TimeSpan.FromTicks(lastCycleTime.Ticks * (this.RequiredCycles - this.PerformedCycles));
                    }
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

        private void UpdateStatusButtonFooter(bool force = false)
        {
            if (!this.IsVisible && !force)
            {
                return;
            }

            switch (this.CurrentStep)
            {
                case CarouselCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case CarouselCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case CarouselCalibrationStep.ConfirmAdjustment:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
        }

        #endregion
    }
}
