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
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum ExternalBayCalibrationStep
    {
        CallUnit,

        StartCalibration,

        RunningCalibration,

        ConfirmAdjustment,
    }

    [Warning(WarningsArea.Installation)]
    public class ExternalBayCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineExternalBayWebService machineExternalBayWebMachine;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand applyCommand;

        private DelegateCommand callLoadUnitToBayCommand;

        private bool canLoadingUnitId;

        private DelegateCommand completeCommand;

        private double? currentDistance;

        private double? currentResolution;

        private ExternalBayCalibrationStep currentStep;

        private double? cyclesPercent;

        private TimeSpan firstCycleTime = default(TimeSpan);

        private bool isCalibrationCompletedOrStopped;

        private bool isCalibrationNotCompleted;

        private bool isChainOffsetVisible;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isExecutingProcedure;

        private bool isExecutingStopInPhase;

        private bool isNewErrorValueVisible;

        private bool isTuningBay;

        private int? loadingUnitId = 0;

        private DelegateCommand moveToStartCalibrationCommand;

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

        private DelegateCommand stopInPhaseCommand;

        private SubscriptionToken themeChangedToken;

        private DelegateCommand tuningBayCommand;

        #endregion

        #region Constructors

        public ExternalBayCalibrationViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineExternalBayWebService machineExternalBayWebMachine,
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineBaysWebService machineBaysWebService)
          : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineExternalBayWebMachine = machineExternalBayWebMachine ?? throw new ArgumentNullException(nameof(machineExternalBayWebMachine));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

            this.CurrentStep = ExternalBayCalibrationStep.CallUnit;
        }

        #endregion

        #region Properties

        public ICommand ApplyCommand =>
            this.applyCommand
            ??
            (this.applyCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(), this.CanApply));

        public ICommand CallLoadUnitToBayCommand =>
                    this.callLoadUnitToBayCommand
            ??
            (this.callLoadUnitToBayCommand = new DelegateCommand(
                async () => await this.CallLoadUnitToBayCommandAsync(),
                this.CanCallLoadUnitToBay));

        public bool CanLoadingUnitId
        {
            get => this.canLoadingUnitId;
            private set => this.SetProperty(ref this.canLoadingUnitId, value);
        }

        public double? ChainOffset => Math.Abs(this.MachineService.Bay.ChainOffset);

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

        public ExternalBayCalibrationStep CurrentStep
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

        public bool HasStepCallUnit => this.currentStep is ExternalBayCalibrationStep.CallUnit;

        public bool HasStepConfirmAdjustment => this.currentStep is ExternalBayCalibrationStep.ConfirmAdjustment;

        public bool HasStepRunningCalibration => this.currentStep is ExternalBayCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is ExternalBayCalibrationStep.StartCalibration;

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

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public ICommand MoveToStartCalibration =>
                           this.moveToStartCalibrationCommand
           ??
           (this.moveToStartCalibrationCommand = new DelegateCommand(
               () => this.CurrentStep = ExternalBayCalibrationStep.StartCalibration,
               this.CanMoveToStartCalibration));

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
                    this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
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
                async () => await this.StopInPhaseAsync(),
                this.CanStopInPhase));

        public ICommand TuningBayCommand =>
            this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuneBayAsync(),
                this.CanTuneBay));

        protected External ProcedureParameters { get; private set; }

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

                    case nameof(this.LoadingUnitId):
                        if (!this.LoadingUnitId.HasValue ||
                            (!this.MachineService.Loadunits.DrawerInLocationById(this.LoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInElevatorById(this.LoadingUnitId.Value) &&
                             !this.MachineService.Loadunits.DrawerInBayById(this.LoadingUnitId.Value)))
                        {
                            return VW.App.Resources.Localized.Get("InstallationApp.InvalidDrawerSelected");
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

            if ((this.MachineService.Bay.IsDouble && this.MachineStatus.LoadingUnitPositionUpInBay != null) ||
                (!this.MachineService.Bay.IsDouble && ((this.MachineService.BayFirstPositionIsUpper && this.MachineStatus.LoadingUnitPositionUpInBay != null) ||
                                                       (!this.MachineService.BayFirstPositionIsUpper && this.MachineStatus.LoadingUnitPositionDownInBay != null))))
            {
                if (this.MachineStatus.LoadingUnitPositionUpInBay != null)
                {
                    this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionUpInBay.Id;
                }
                else
                {
                    this.LoadingUnitId = this.MachineStatus.LoadingUnitPositionDownInBay.Id;
                }
            }
            else
            {
                this.LoadingUnitId = 1;
            }

            this.UpdateStatusButtonFooter(true);

            await this.RetrieveProcedureInformationAsync();

            this.RaiseCanExecuteChanged();

            await base.OnAppearedAsync();
        }

        public async Task RetrieveProcedureInformationAsync()
        {
            try
            {
                var procedureParameters = await this.machineExternalBayWebMachine.GetParametersAsync();

                this.RequiredCycles = procedureParameters.RequiredCycles;
                this.PerformedCycles = procedureParameters.PerformedCycles;

                if (this.RequiredCycles != 0)
                {
                    this.CyclesPercent = (this.PerformedCycles / this.RequiredCycles) * 100;
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

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                await this.RetrieveProcedureInformationAsync();

                // devo controllare che non sia cambiata dai parametri o altre baie
                this.CurrentResolution = this.MachineService.Bay.Resolution;

                this.CurrentDistance = this.MachineService.Bay.External.Race;

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
                case ExternalBayCalibrationStep.CallUnit:
                    if (e.Next)
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
                    }

                    break;

                case ExternalBayCalibrationStep.StartCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.RunningCalibration;
                    }
                    else
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.CallUnit;
                    }

                    break;

                case ExternalBayCalibrationStep.RunningCalibration:
                    if (e.Next)
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.ConfirmAdjustment;
                    }
                    else
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
                    }

                    break;

                case ExternalBayCalibrationStep.ConfirmAdjustment:
                    if (!e.Next)
                    {
                        this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
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

            this.CanLoadingUnitId = this.CanBaseExecute() &&
                                    !this.SensorsService.IsLoadingUnitInBay;
            this.callLoadUnitToBayCommand?.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
            this.stopInPhaseCommand?.RaiseCanExecuteChanged();

            this.repeatCalibrationCommand?.RaiseCanExecuteChanged();
            this.startCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToStartCalibrationCommand?.RaiseCanExecuteChanged();
            this.applyCommand?.RaiseCanExecuteChanged();
            this.completeCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.RemainingTime));
            this.RaisePropertyChanged(nameof(this.PerformedCycles));
            this.RaisePropertyChanged(nameof(this.CyclesPercent));
            this.RaisePropertyChanged(nameof(this.RequiredCycles));
            this.RaisePropertyChanged(nameof(this.IsExecutingProcedure));
            this.RaisePropertyChanged(nameof(this.IsExecutingStopInPhase));

            this.RaisePropertyChanged(nameof(this.NewErrorValue));
            this.RaisePropertyChanged(nameof(this.ChainOffset));
        }

        private async Task ApplyCorrectionAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ApplyCorrectionMessage"), Localized.Get("InstallationApp.CarouselCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    double newRaceDistance = 0;

                    if (this.IsNewErrorValueVisible)
                    {
                        var measuredCorrection = this.IsErrorNegative ? this.NewErrorValue : -this.NewErrorValue;
                        var correctionForEachMovement = measuredCorrection / this.SessionPerformedCycles;
                        newRaceDistance = (double)correctionForEachMovement + this.MachineService.Bay.External.Race;
                    }
                    else
                    {
                        var measuredCorrection = this.IsErrorNegative ? this.ChainOffset : -this.ChainOffset;
                        var correctionForEachMovement = measuredCorrection / this.SessionPerformedCycles;
                        newRaceDistance = (double)correctionForEachMovement + this.MachineService.Bay.External.Race;
                    }

                    await this.machineExternalBayWebMachine.UpdateRaceDistanceAsync(newRaceDistance);

                    await this.MachineService.OnUpdateServiceAsync();

                    this.CurrentDistance = this.MachineService.Bay.External.Race;

                    this.ShowNotification(
                            VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                            Services.Models.NotificationSeverity.Success);

                    this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
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

        private async Task CallLoadUnitToBayCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.EjectLoadingUnitAsync(this.MachineService.GetBayPositionSourceByDestination(false), this.LoadingUnitId.Value);
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

        private bool CanCallLoadUnitToBay()
        {
            return this.CanBaseExecute() &&
                !this.MachineService.Loadunits.DrawerInBay() &&
                this.MachineService.Loadunits.DrawerExists(this.loadingUnitId.Value) &&
                   !this.SensorsService.IsLoadingUnitInBay;
        }

        private bool CanComplete()
        {
            return this.CanBaseExecute();
        }

        private bool CanMoveToStartCalibration()
        {
            return this.CanBaseExecute();
            //&& this.SensorsService.Sensors.LUPresentInBay1; //this.SensorsService.Sensors.LUPresentMiddleBottomBay1;
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
                   !this.SensorsService.IsHorizontalInconsistentBothHigh; // && this.SensorsService.BayZeroChain;
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
                   this.MachineStatus.LoadingUnitPositionUpInBay is null &&
                   this.SensorsService.BayZeroChain
                   ;
        }

        private async Task CompleteAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmCalibrationProcedure"), Localized.Get("InstallationApp.CarouselCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.IsExecutingStopInPhase = false;
                    this.IsExecutingProcedure = false;

                    await this.machineExternalBayWebMachine.SetCalibrationCompletedAsync();

                    this.ShowNotification(
                        VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"),
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
                this.CurrentStep = ExternalBayCalibrationStep.StartCalibration;
                this.IsWaitingForResponse = false;
            }
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode != MovementMode.ExtBayTest)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored()
                ||
                this.MachineError != null)
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);

                this.IsCalibrationCompletedOrStopped = false;

                this.IsChainOffsetVisible = true;
                this.IsNewErrorValueVisible = false;

                this.IsExecutingProcedure = false;

                this.IsCalibrationNotCompleted = false;

                this.CurrentStep = ExternalBayCalibrationStep.ConfirmAdjustment;

                if (this.MachineError != null)
                {
                    this.IsChainOffsetVisible = false;
                    this.IsNewErrorValueVisible = false;
                    this.IsCalibrationNotCompleted = true;
                }

                return;
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
                    this.ShowNotification(Localized.Get("InstallationApp.TestStopped"), Services.Models.NotificationSeverity.Success);
                }
                else
                {
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);
                }

                this.IsCalibrationNotCompleted = false;

                this.IsChainOffsetVisible = false;
                this.IsNewErrorValueVisible = true;

                this.IsExecutingStopInPhase = false;
                this.IsExecutingProcedure = false;

                this.IsCalibrationCompletedOrStopped = true;

                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.NewErrorValue = 0;
                this.CurrentStep = ExternalBayCalibrationStep.ConfirmAdjustment;
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
                this.PerformedCycles = message.Data.ExecutedCycles;
                this.SessionPerformedCycles = this.PerformedCycles - this.StartPerformedCycles;

                this.IsCalibrationCompletedOrStopped = false;
                this.NewErrorValue = 0;

                this.CurrentStep = ExternalBayCalibrationStep.ConfirmAdjustment;
                this.RaiseCanExecuteChanged();

                this.IsExecutingProcedure = false;
            }
        }

        private async Task StartCalibrationAsync()
        {
            this.IsWaitingForResponse = true;
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.StartProcedureMessage"), Localized.Get("InstallationApp.ExtBayCalibration"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineExternalBayWebMachine.ResetCalibrationAsync();

                    await this.RetrieveProcedureInformationAsync();

                    // Update procedure info
                    this.StartPerformedCycles = this.PerformedCycles;
                    this.SessionPerformedCycles = 0;

                    this.oldPerformedCycle = this.PerformedCycles;
                    this.startTime = DateTime.Now;

                    if (this.RemainingTime != null)
                    {
                        this.RemainingTime = default(TimeSpan);
                    }

                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    await this.machineExternalBayWebMachine.StartCalibrationAsync();

                    this.CurrentStep = ExternalBayCalibrationStep.RunningCalibration;
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
                await this.machineExternalBayWebMachine.StopCalibrationAsync();
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
                           this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
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
                    await this.machineExternalBayWebMachine.FindZeroAsync();

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
                case ExternalBayCalibrationStep.CallUnit:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToStartCalibrationCommand?.CanExecute() ?? false);
                    break;

                case ExternalBayCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case ExternalBayCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                case ExternalBayCalibrationStep.ConfirmAdjustment:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepCallUnit));
            this.RaisePropertyChanged(nameof(this.HasStepStartCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepRunningCalibration));
            this.RaisePropertyChanged(nameof(this.HasStepConfirmAdjustment));
        }

        #endregion
    }
}
