using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Resources;
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

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand applyCommand;

        private double axisLowerBound;

        private double axisUpperBound;

        private double currentDistance;

        private double? currentResolution;

        private CarouselCalibrationStep currentStep;

        private double? cyclesPercent;

        private bool isErrorNegative = true;

        private bool isErrorPositive = false;

        private bool isExecutingProcedure;

        private bool isTuningBay;

        private DelegateCommand moveToConfirmAdjustmentCommand;

        private DelegateCommand moveToRunningCalibrationCommand;

        private DelegateCommand moveToStartCalibrationCommand;

        private double? newErrorValue;

        private int performedCycles;

        private int requiredCycles;

        private DelegateCommand resetCyclesCounterCommand;

        private DelegateCommand saveCommand;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private DelegateCommand stopInPhaseCommand;

        private DelegateCommand tuningBayCommand;

        #endregion

        #region Constructors

        public CarouselCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IDialogService dialogService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineBaysWebService machineBaysWebService
            )
          : base(PresentationMode.Installer)
        {
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
                async () => await this.ApplyCorrectionAsync()));

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

        public String CompletionStatus
        {
            get => "Reset cicli " + this.performedCycles.ToString() + " di " + this.requiredCycles.ToString();
        }

        public double CurrentDistance
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
            protected set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => throw new NotImplementedException();

        public bool HasStepConfirmAdjustment => this.currentStep is CarouselCalibrationStep.ConfirmAdjustment;

        public bool HasStepRunningCalibration => this.currentStep is CarouselCalibrationStep.RunningCalibration;

        public bool HasStepStartCalibration => this.currentStep is CarouselCalibrationStep.StartCalibration;

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
            protected set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set => this.SetProperty(ref this.isTuningBay, value);
        }

        public ICommand MoveStartCalibrationCommand =>
            this.moveToStartCalibrationCommand
            ??
            (this.moveToStartCalibrationCommand = new DelegateCommand(
                () => this.CurrentStep = CarouselCalibrationStep.StartCalibration,
                this.CanToStartCalibration));

        public ICommand MoveToConfirmAdjustmentCommand =>
                    this.moveToConfirmAdjustmentCommand
            ??
            (this.moveToConfirmAdjustmentCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep = CarouselCalibrationStep.ConfirmAdjustment;
                },
                this.CanToConfirmAdjustment));

        public ICommand MoveToRunningCalibrationCommand =>
                   this.moveToRunningCalibrationCommand
           ??
           (this.moveToRunningCalibrationCommand = new DelegateCommand(
               () =>
               {
                   this.CurrentStep = CarouselCalibrationStep.RunningCalibration;
                   this.CalibrationCarouselAsync();
               },
               this.CanToRunningCalibration));

        public double? NewErrorValue
        {
            get => this.newErrorValue;
            set => this.SetProperty(ref this.newErrorValue, value, this.RaiseCanExecuteChanged);
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

        public ICommand ResetCyclesCounterCommand =>
            this.resetCyclesCounterCommand
          ??
          (this.resetCyclesCounterCommand = new DelegateCommand(
              async () => await this.ResetCyclesCounterAsync(),
              this.CanExecuteResetCyclesCounterCommand));

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync()));

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
                this.CurrentStep = CarouselCalibrationStep.StartCalibration;
                this.StopInPhaseAsync();
            },
            this.CanToStopInPhase));

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
                    //case nameof(this.DestinationPosition1):
                    //    if (this.CurrentStep == CalibrationStep.FirstMeasured &&
                    //        !this.IsMoving)
                    //    {
                    //        if (!this.DestinationPosition1.HasValue)
                    //        {
                    //            this.currentError = InstallationApp.DestinationPositionRequired;
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }

                    //        if (this.DestinationPosition1.Value < 0)
                    //        {
                    //            this.currentError = InstallationApp.DestinationPositionMustBePositive;
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }

                    //        if (this.DestinationPosition1.Value < this.axisLowerBound ||
                    //            this.DestinationPosition1.Value > this.axisUpperBound)
                    //        {
                    //            this.currentError = string.Format(InstallationApp.DestinationPositionOutOfRangeAxis, this.AxisLowerBound, this.AxisUpperBound);
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }
                    //    }

                    //    break;

                    //case nameof(this.MeasuredPosition1):
                    //    if (this.CurrentStep == CalibrationStep.FirstMeasured &&
                    //        !this.IsMoving)
                    //    {
                    //        if (this.MeasuredPosition1.HasValue &&
                    //            (this.MeasuredPosition1.Value < this.axisLowerBound ||
                    //             this.MeasuredPosition1.Value > this.axisUpperBound) &&
                    //            Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.DestinationPosition1.Value))
                    //        {
                    //            this.currentError = string.Format(InstallationApp.MeasuredPositionOutOfRangeAxis, this.AxisLowerBound, this.AxisUpperBound);
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }
                    //    }

                    //    break;

                    //case nameof(this.StartPosition):
                    //    if (this.CurrentStep == CalibrationStep.PositionMeter &&
                    //        !this.IsMoving)
                    //    {
                    //        if (this.StartPosition < 0)
                    //        {
                    //            this.currentError = InstallationApp.StartPositionMustBePositive;
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }

                    //        if ((this.StartPosition < this.axisLowerBound ||
                    //            this.StartPosition > this.axisUpperBound) &&
                    //            this.axisLowerBound > 0 &&
                    //            this.axisUpperBound > 0)
                    //        {
                    //            this.currentError = string.Format(InstallationApp.StartPositionOutOfRangeAxis, this.AxisLowerBound, this.AxisUpperBound);
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }
                    //    }

                    //    break;

                    //case nameof(this.MeasuredPosition2):
                    //case nameof(this.DestinationPosition2):
                    //    if (this.CurrentStep == CalibrationStep.LastMeasured &&
                    //        !this.IsMoving)
                    //    {
                    //        if (columnName.Equals(nameof(this.DestinationPosition2)))
                    //        {
                    //            if (!this.DestinationPosition2.HasValue)
                    //            {
                    //                this.currentError = InstallationApp.DestinationPositionRequired;
                    //                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //                return this.currentError;
                    //            }

                    //            if (this.DestinationPosition2.Value < 0)
                    //            {
                    //                this.currentError = InstallationApp.DestinationPositionMustBePositive;
                    //                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //                return this.currentError;
                    //            }

                    //            if (this.DestinationPosition2.Value < this.axisLowerBound ||
                    //                this.DestinationPosition2.Value > this.axisUpperBound)
                    //            {
                    //                this.currentError = string.Format(InstallationApp.DestinationPositionOutOfRangeAxis, this.AxisLowerBound, this.AxisUpperBound);
                    //                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //                return this.currentError;
                    //            }
                    //        }

                    //        if (this.MeasuredPosition2.HasValue &&
                    //            (this.MeasuredPosition2.Value < this.axisLowerBound ||
                    //             this.MeasuredPosition2.Value > this.axisUpperBound) &&
                    //            Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.DestinationPosition2.Value))
                    //        {
                    //            this.currentError = string.Format(InstallationApp.MeasuredPositionOutOfRangeAxis, this.AxisLowerBound, this.AxisUpperBound);
                    //            this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                    //            return this.currentError;
                    //        }
                    //    }

                    //    break;
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

            if (this.sensorsToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.sensorsToken);
                this.sensorsToken?.Dispose();
                this.sensorsToken = null;
            }

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                // devo controllare che non sia cambiata dai parametri o altre baie
                this.CurrentResolution = await this.machineElevatorWebService.GetVerticalResolutionAsync();

                //if (this.AxisUpperBound == 0 || this.AxisLowerBound == 0 || this.StartPosition == 0 || !this.DestinationPosition1.HasValue || !this.DestinationPosition2.HasValue)
                //{
                //    var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();
                //    this.ProcedureParameters = await this.resolutionCalibrationWebService.GetParametersAsync();

                //    this.StartPosition = this.ProcedureParameters.StartPosition;
                //    this.DestinationPosition1 = this.ProcedureParameters.InitialPosition;
                //    this.DestinationPosition2 = this.ProcedureParameters.FinalPosition;
                //    this.AxisUpperBound = procedureParameters.UpperBound;
                //    this.AxisLowerBound = procedureParameters.LowerBound;
                //}
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

            this.moveToStartCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToRunningCalibrationCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmAdjustmentCommand?.RaiseCanExecuteChanged();
            this.applyCommand?.RaiseCanExecuteChanged();
            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                //await this.machineElevatorWebService.UpdateVerticalResolutionAsync(this.NewResolution.Value);

                //this.CurrentResolution = this.NewResolution;
                //this.MeasuredPosition1 = null;
                //this.MeasuredPosition2 = null;
                //this.CurrentStep = CalibrationStep.PositionMeter;

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
                    Services.Models.NotificationSeverity.Success);

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

        private async Task CalibrationCarouselAsync()
        {
            try
            {
                await this.machineCarouselWebService.StartCalibrationAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
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
                   !this.SensorsService.IsHorizontalInconsistentBothHigh &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanExecuteResetCyclesCounterCommand()
        {
            return /*this.PerformedCycles.HasValue &&*/
                   this.PerformedCycles > 0 &&
                   !this.IsExecutingProcedure &&
                   string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanSave()
        {
            return this.CanBaseExecute();
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanToConfirmAdjustment()
        {
            return this.CanBaseExecute();
        }

        private bool CanToRunningCalibration()
        {
            return this.CanBaseExecute();
        }

        private bool CanToStartCalibration()
        {
            return this.CanBaseExecute();
        }

        private bool CanToStopInPhase()
        {
            return this.CanBaseExecute();
        }

        private bool CanTuneBay()
        {
            return this.CanBaseExecute() &&
                   !this.IsTuningBay &&
                   this.MachineStatus.LoadingUnitPositionDownInBay is null &&
                   this.MachineStatus.LoadingUnitPositionUpInBay is null &&
                   this.SensorsService.Sensors.ACUBay1S3IND;
        }

        private async Task ResetCyclesCounterAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Desideri resettare i cicli eseguiti?", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.PerformedCycles = 0;

                    //await this.shuttersWebService.ResetTestAsync();
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
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopInPhaseAsync()
        {
            try
            {
                await this.machineCarouselWebService.StopCalibrationAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
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

            //this.sensorsToken = this.sensorsToken ??
            //    this.EventAggregator
            //        .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
            //        .Subscribe(
            //            async (m) => await this.OnSensorsChangedAsync(m),
            //            ThreadOption.UIThread,
            //            false,
            //            (m) =>
            //            {
            //                return !this.luPresentInOperatorSide.HasValue ||
            //                       !this.luPresentInMachineSide.HasValue ||
            //                       (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInOperatorSide] != this.luPresentInOperatorSide.Value) ||
            //                       (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInMachineSide] != this.luPresentInMachineSide.Value);
            //            });
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, InstallationApp.BayCalibration, DialogType.Question, DialogButtons.YesNo);
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

        //private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        //{
        //    this.luPresentInOperatorSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
        //    this.luPresentInMachineSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
        //}

        //private async void RetrieveNewResolutionAsync()
        //{
        //    if (this.MeasuredPosition1.HasValue &&
        //        this.MeasuredPosition2.HasValue)
        //    {
        //        this.MeasuredDistance = this.MeasuredPosition2 - this.MeasuredPosition1;

        //        this.tokenSource?.Cancel(false);
        //        this.tokenSource = new CancellationTokenSource();

        //        try
        //        {
        //            const int callDelayMilliseconds = 300;

        //            await Task
        //                .Delay(callDelayMilliseconds, this.tokenSource.Token)
        //                .ContinueWith(
        //                    async t => await this.RetrieveNewResolutionAsync(this.tokenSource.Token),
        //                    this.tokenSource.Token,
        //                    TaskContinuationOptions.NotOnCanceled,
        //                    TaskScheduler.Current)
        //                .ConfigureAwait(true);
        //        }
        //        catch (TaskCanceledException)
        //        {
        //            this.IsRetrievingNewResolution = false;
        //        }
        //    }
        //}

        //private async Task RetrieveNewResolutionAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var exepectedDistance = this.DestinationPosition2.Value - this.DestinationPosition1.Value;

        //        this.IsRetrievingNewResolution = true;

        //        this.NewResolution = await this.resolutionCalibrationWebService
        //            .GetAdjustedResolutionAsync(
        //                this.MeasuredDistance.Value,
        //                exepectedDistance,
        //                cancellationToken);

        //        this.IsRetrievingNewResolution = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ShowNotification(ex);
        //        this.NewResolution = null;
        //    }
        //}

        //private async Task StartAsync(double position)
        //{
        //    try
        //    {
        //        this.IsWaitingForResponse = true;
        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case CarouselCalibrationStep.StartCalibration:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToRunningCalibrationCommand?.CanExecute() ?? false);
                    break;

                case CarouselCalibrationStep.RunningCalibration:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, true);// this.moveToConfirmAdjustmentCommand?.CanExecute() ?? false);
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
