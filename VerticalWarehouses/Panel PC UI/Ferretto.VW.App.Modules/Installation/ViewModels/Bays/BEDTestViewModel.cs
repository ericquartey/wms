using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum BEDTestStep
    {
        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment,
    }

    [Warning(WarningsArea.Installation)]
    public class BEDTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineExternalBayWebService machineExternalBayWebService;

        private double? currentDistance;

        private double? currentResolution;

        private CarouselCalibrationStep currentStep;

        private int cyclesPercent;

        private TimeSpan firstCycleTime = default(TimeSpan);

        private bool isBeltButnishing = false;

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isChainOffsetVisible;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isExecutingProcedure;

        private bool isNewErrorValueVisible;

        private bool isTuningBay;

        private int? newErrorValue;

        private int oldPerformedCycle = 0;

        private int performedCycles;

        private SubscriptionToken positioningMessageReceivedToken;

        private TimeSpan remainingTime = default(TimeSpan);

        private DelegateCommand repeatCalibrationCommand;

        private int requiredCycles;

        private int sessionPerformedCycles;

        private long singleRaisingTicks = 0;

        private DelegateCommand startCalibrationCommand;

        private int startPerformedCycles;

        private DateTime startTime = DateTime.Now;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        private DelegateCommand tuningBayCommand;

        #endregion

        #region Constructors

        public BEDTestViewModel(
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            IMachineExternalBayWebService machineExternalBayWebService)
          : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineExternalBayWebService = machineExternalBayWebService ?? throw new ArgumentNullException(nameof(machineExternalBayWebService));

            this.CurrentStep = CarouselCalibrationStep.StartCalibration;
        }

        #endregion

        #region Properties

        public double? ChainOffset => Math.Abs(this.MachineService.Bay.ChainOffset);

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

        public int CyclesPercent
        {
            get => this.cyclesPercent;
            set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error => string.Join(
            this[nameof(this.CurrentDistance)],
            this[nameof(this.CurrentResolution)],
            this[nameof(this.NewErrorValue)]);

        public bool HasCarousel => this.MachineService.HasCarousel;

        public bool HasShutter => this.MachineService.HasShutter;

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
                    this.UpdateView();
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
                    this.UpdateView();
                }
            }
        }

        //public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsNewErrorValueVisible
        {
            get => this.isNewErrorValueVisible;
            set => this.SetProperty(ref this.isNewErrorValueVisible, value);
        }

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set => this.SetProperty(ref this.isTuningBay, value);
        }

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
                    this.UpdateView();
                }
            }
        }

        public TimeSpan RemainingTime
        {
            get => new TimeSpan(this.remainingTime.Hours, this.remainingTime.Minutes, this.remainingTime.Seconds);
            set => this.SetProperty(ref this.remainingTime, value, this.UpdateView);
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
                    this.UpdateView();
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
                    this.UpdateView();
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
                    this.UpdateView();
                }
            }
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        public ICommand TuningBayCommand =>
            this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuneBayAsync(),
                this.CanTuneBay));

        protected MAS.AutomationService.Contracts.RepeatedTestProcedure ProcedureParameters { get; private set; }

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

            this.UpdateStatusButtonFooter(true);

            //await this.RetrieveProcedureInformationAsync();

            this.UpdateView();

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.machineExternalBayWebService.GetParametersAsync();

                this.RequiredCycles = procedureParameters.RequiredCycles;
                this.PerformedCycles = procedureParameters.PerformedCycles;
                this.CyclesPercent = (this.performedCycles * 100) / this.requiredCycles;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
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

                this.IsExecutingProcedure = this.MachineService.MachineStatus.IsMoving ||
                    this.MachineService.MachineMode == MachineMode.Test ||
                    this.MachineService.MachineMode == MachineMode.Test2 ||
                    this.MachineService.MachineMode == MachineMode.Test3;

                this.UpdateView();
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

            this.UpdateView();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();
            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh;
        }

        private bool CanRepeat()
        {
            return this.CanBaseExecute();
        }

        private bool CanStartCalibration()
        {
            return (!this.IsMoving || this.isBeltButnishing) &&
                   (this.SensorsService.BayZeroChain || this.SensorsService.BayZeroChainUp);
        }

        private bool CanStop()
        {
            return this.IsMoving ||
                this.IsExecutingProcedure;
        }

        private bool CanTuneBay()
        {
            return (!this.IsMoving || this.isBeltButnishing) &&
                   !this.IsTuningBay &&
                   this.MachineStatus.LoadingUnitPositionDownInBay is null &&
                   this.MachineStatus.LoadingUnitPositionUpInBay is null &&
                   this.SensorsService.BayZeroChain
                   ;
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode == MovementMode.BeltBurnishing)
            {
                this.isBeltButnishing = true;
            }

            if (message.Data?.MovementMode != MovementMode.DoubleExtBayTest)
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
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = true;

                this.IsChainOffsetVisible = true;
                this.IsNewErrorValueVisible = false;

                this.IsExecutingProcedure = false;

                this.IsCalibrationNotCompleted = this.requiredCycles == this.performedCycles ? false : true;

                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
                    this.IsChainOffsetVisible = false;
                    this.IsNewErrorValueVisible = true;
                    this.IsCalibrationNotCompleted = true;
                    this.NewErrorValue = 0;
                }

                return;
            }

            // ad ogni ciclo completato...aggiornamento dati
            if (message.Status == MessageStatus.OperationExecuting)
            {
                // update cycle info
                this.RequiredCycles = message.Data.RequiredCycles;
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.UpdateRemainingTime();
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.PerformedCycles = message.Data.ExecutedCycles;

                if (message.Data.IsTestStopped)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.TestStopped"), Services.Models.NotificationSeverity.Success);

                    this.IsNewErrorValueVisible = false;

                    this.IsCalibrationNotCompleted = true;
                }
                else
                {
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);

                    this.IsNewErrorValueVisible = true;

                    this.IsCalibrationNotCompleted = this.requiredCycles == this.performedCycles ? false : true;
                }

                this.IsChainOffsetVisible = false;

                this.IsExecutingProcedure = false;

                this.IsCalibrationCompletedOrStopped = true;

                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.NewErrorValue = 0;
                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                this.UpdateView();
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsCalibrationNotCompleted = true;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = false;

                this.IsExecutingProcedure = false;
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = 0;

                this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                this.UpdateView();

                this.IsExecutingProcedure = false;
            }

            this.CyclesPercent = (this.performedCycles * 100) / this.requiredCycles;
        }

        private async Task StartCalibrationAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.StartProcedureMessage"), "Test BED", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineExternalBayWebService.ResetCalibrationAsync();

                    var procedureParameters = await this.machineExternalBayWebService.GetParametersAsync();
                    if (procedureParameters.RequiredCycles != this.requiredCycles)
                    {
                        await this.machineExternalBayWebService.UpdateProcedureCycleAsync(this.requiredCycles);
                    }

                    await this.RetrieveProcedureInformationAsync();

                    // Update procedure info
                    this.StartPerformedCycles = this.PerformedCycles;
                    this.SessionPerformedCycles = 0;

                    this.CyclesPercent = 0;

                    this.oldPerformedCycle = this.PerformedCycles;
                    this.startTime = DateTime.Now;

                    if (this.RemainingTime != null)
                    {
                        this.RemainingTime = default(TimeSpan);
                    }

                    this.IsExecutingProcedure = true;
                    this.UpdateView();

                    if (this.SensorsService.BayZeroChain)
                    {
                        await this.machineExternalBayWebService.StartDoubleExtBayTestAsync(ExternalBayMovementDirection.TowardOperator, false);
                    }
                    else if (this.SensorsService.BayZeroChainUp)
                    {
                        await this.machineExternalBayWebService.StartDoubleExtBayTestAsync(ExternalBayMovementDirection.TowardMachine, true);
                    }

                    this.CurrentStep = CarouselCalibrationStep.RunningCalibration;
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsWaitingForResponse = false;
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

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
                           this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.BayCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsTuningBay = true;
                    await this.machineExternalBayWebService.FindZeroAsync();

                    this.IsExecutingProcedure = true;
                }
            }
            catch (Exception ex)
            {
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
                    this.singleRaisingTicks = 0;

                    if (this.firstCycleTime != null)
                    {
                        this.firstCycleTime = default(TimeSpan);
                    }

                    if (this.RemainingTime != null)
                    {
                        this.RemainingTime = default(TimeSpan);
                    }
                }
                else
                {
                    if (this.PerformedCycles > this.oldPerformedCycle)
                    {
                        this.oldPerformedCycle = this.PerformedCycles;

                        // escludiamo il primo in quanto è affetto da ritardi (8s vs 4s circa)
                        if (this.PerformedCycles == 1)
                        {
                            this.firstCycleTime = DateTime.Now - this.startTime;
                        }
                        else
                        {
                            // ogni 30 cicli ricalcolo il tempo per singolo ciclo...
                            if ((this.PerformedCycles % 30) == 0 || this.PerformedCycles == 2)
                            {
                                var totalCycleTime = DateTime.Now - this.startTime - this.firstCycleTime;

                                this.singleRaisingTicks = totalCycleTime.Ticks / (this.PerformedCycles - 1);
                            }
                        }

                        this.RemainingTime = TimeSpan.FromTicks(this.singleRaisingTicks * (this.RequiredCycles - this.PerformedCycles));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
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

        private void UpdateView()
        {
            base.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();
            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.RemainingTime));
            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.CyclesPercent));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        #endregion
    }
}
