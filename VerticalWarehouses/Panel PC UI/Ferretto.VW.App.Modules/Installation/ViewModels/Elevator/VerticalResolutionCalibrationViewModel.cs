using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum CalibrationStep
    {
        PositionMeter,

        FirstMeasured,

        LastMeasured,

        Confirm,
    }

    [Warning(WarningsArea.Installation)]
    public class VerticalResolutionCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService;

        private readonly IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService;

        private double axisLowerBound;

        private double axisUpperBound;

        private string currentError;

        private double? currentResolution;

        private CalibrationStep currentStep;

        private double? destinationPosition1;

        private double? destinationPosition2;

        private bool isExecutingProcedure;

        private bool isRetrievingNewResolution;

        private bool? luPresentInMachineSide;

        private bool? luPresentInOperatorSide;

        private double? measuredDistance;

        private double? mesurationPosition1;

        private double? mesurationPosition2;

        private DelegateCommand moveToConfirmCommand;

        private DelegateCommand moveToFirstMeasuredCommand;

        private DelegateCommand moveToLastMeasuredCommand;

        private DelegateCommand moveToStartDestination1Command;

        private DelegateCommand moveToStartDestination2Command;

        private DelegateCommand moveToStartPositionCommand;

        private double? newResolution;

        private DelegateCommand saveCommand;

        private SubscriptionToken sensorsToken;

        private double startPosition = 0;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private SubscriptionToken themeChangedToken;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineVerticalOriginProcedureWebService verticalOriginProcedureWebService,
            IDialogService dialogService)
          : base(PresentationMode.Installer)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.verticalOriginProcedureWebService = verticalOriginProcedureWebService ?? throw new ArgumentNullException(nameof(verticalOriginProcedureWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.CurrentStep = CalibrationStep.PositionMeter;
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

        public double? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public CalibrationStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public double? DestinationPosition1
        {
            get => this.destinationPosition1;
            set => this.SetProperty(ref this.destinationPosition1, value,
                () =>
                {
                    this.currentError = string.Empty;
                    this.MeasuredPosition1 = null;
                    this.RaiseCanExecuteChanged();
                });
        }

        public double? DestinationPosition2
        {
            get => this.destinationPosition2;
            set => this.SetProperty(ref this.destinationPosition2, value,
                () =>
                {
                    this.currentError = string.Empty;
                    this.MeasuredPosition2 = null;
                    this.RaiseCanExecuteChanged();
                });
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            this[nameof(this.DestinationPosition1)],
            this[nameof(this.DestinationPosition2)],
            this[nameof(this.MeasuredPosition1)],
            this[nameof(this.MeasuredPosition2)],
            this[nameof(this.StartPosition)]);

        public bool HasStepConfirm => this.currentStep is CalibrationStep.Confirm;

        public bool HasStepFirstMeasured => this.currentStep is CalibrationStep.FirstMeasured;

        public bool HasStepLastMeasured => this.currentStep is CalibrationStep.LastMeasured;

        public bool HasStepPositionMeter => this.currentStep is CalibrationStep.PositionMeter;

        public bool IsCanDestinationPosition1 => this.CanBaseExecute();

        public bool IsCanDestinationPosition2 => this.CanBaseExecute();

        public bool IsCanMeasuredPosition1 => this.CanBaseExecute();

        public bool IsCanMeasuredPosition2 => this.CanBaseExecute();

        public bool IsCanStartPosition => this.CanBaseExecute();

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMoving => (this.MachineService?.MachineStatus?.IsMoving ?? true) || (this.MachineService?.MachineStatus?.IsMovingLoadingUnit ?? true);

        public bool IsRetrievingNewResolution
        {
            get => this.isRetrievingNewResolution;
            set => this.SetProperty(ref this.isRetrievingNewResolution, value);
        }

        public double? MeasuredDistance
        {
            get => this.measuredDistance;
            set => this.SetProperty(ref this.measuredDistance, value);
        }

        public double? MeasuredPosition1
        {
            get => this.mesurationPosition1;
            set => this.SetProperty(ref this.mesurationPosition1, value, () => { this.currentError = string.Empty; this.RaiseCanExecuteChanged(); });
        }

        public double? MeasuredPosition2
        {
            get => this.mesurationPosition2;
            set => this.SetProperty(ref this.mesurationPosition2, value, () => { this.currentError = string.Empty; this.RaiseCanExecuteChanged(); });
        }

        public ICommand MoveToConfirmCommand =>
            this.moveToConfirmCommand
            ??
            (this.moveToConfirmCommand = new DelegateCommand(
                () =>
                {
                    this.CurrentStep = CalibrationStep.Confirm;
                    this.RetrieveNewResolutionAsync();
                },
                this.CanToConfirm));

        public ICommand MoveToFirstMeasuredCommand =>
            this.moveToFirstMeasuredCommand
            ??
            (this.moveToFirstMeasuredCommand = new DelegateCommand(
                () => this.CurrentStep = CalibrationStep.FirstMeasured,
                this.CanToFirstMeasured));

        public ICommand MoveToLastMeasuredCommand =>
            this.moveToLastMeasuredCommand
            ??
            (this.moveToLastMeasuredCommand = new DelegateCommand(
                () => this.CurrentStep = CalibrationStep.LastMeasured,
                this.CanToLastMeasured));

        public ICommand MoveToStartDestination1Command =>
            this.moveToStartDestination1Command
            ??
            (this.moveToStartDestination1Command = new DelegateCommand(
                async () => await this.StartAsync(this.DestinationPosition1.Value),
                this.CanMoveToStartDestination1));

        public ICommand MoveToStartDestination2Command =>
            this.moveToStartDestination2Command
            ??
            (this.moveToStartDestination2Command = new DelegateCommand(
                async () => await this.StartAsync(this.DestinationPosition2.Value),
                this.CanMoveToStartDestination2));

        public ICommand MoveToStartPositionCommand =>
            this.moveToStartPositionCommand
            ??
            (this.moveToStartPositionCommand = new DelegateCommand(
                async () => await this.StartAsync(this.StartPosition),
                this.CanMoveToStartPosition));

        public double? NewResolution
        {
            get => this.newResolution;
            set => this.SetProperty(ref this.newResolution, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync()));

        public double StartPosition
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
                this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.DestinationPosition1):
                        if (this.CurrentStep == CalibrationStep.FirstMeasured &&
                            !this.IsMoving)
                        {
                            if (!this.DestinationPosition1.HasValue)
                            {
                                this.currentError = Localized.Get("InstallationApp.DestinationPositionRequired");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if (this.DestinationPosition1.Value < 0)
                            {
                                this.currentError = Localized.Get("InstallationApp.DestinationPositionMustBePositive");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if (this.DestinationPosition1.Value < this.axisLowerBound ||
                                this.DestinationPosition1.Value > this.axisUpperBound)
                            {
                                this.currentError = string.Format(Localized.Get("InstallationApp.DestinationPositionOutOfRangeAxis"), this.AxisLowerBound, this.AxisUpperBound);
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
                        }

                        break;

                    case nameof(this.MeasuredPosition1):
                        if (this.CurrentStep == CalibrationStep.FirstMeasured &&
                            !this.IsMoving)
                        {
                            if (this.MeasuredPosition1.HasValue &&
                                (this.MeasuredPosition1.Value < this.axisLowerBound ||
                                 this.MeasuredPosition1.Value > this.axisUpperBound) &&
                                Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.DestinationPosition1.Value))
                            {
                                this.currentError = string.Format(Localized.Get("InstallationApp.MeasuredPositionOutOfRangeAxis"), this.AxisLowerBound, this.AxisUpperBound);
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
                        }

                        break;

                    case nameof(this.StartPosition):
                        if (this.CurrentStep == CalibrationStep.PositionMeter &&
                            !this.IsMoving)
                        {
                            if (this.StartPosition < 0)
                            {
                                this.currentError = Localized.Get("InstallationApp.StartPositionMustBePositive");
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }

                            if ((this.StartPosition < this.axisLowerBound ||
                                this.StartPosition > this.axisUpperBound) &&
                                this.axisLowerBound > 0 &&
                                this.axisUpperBound > 0)
                            {
                                this.currentError = string.Format(Localized.Get("InstallationApp.StartPositionOutOfRangeAxis"), this.AxisLowerBound, this.AxisUpperBound);
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
                        }

                        break;

                    case nameof(this.MeasuredPosition2):
                    case nameof(this.DestinationPosition2):
                        if (this.CurrentStep == CalibrationStep.LastMeasured &&
                            !this.IsMoving)
                        {
                            if (columnName.Equals(nameof(this.DestinationPosition2)))
                            {
                                if (!this.DestinationPosition2.HasValue)
                                {
                                    this.currentError = Localized.Get("InstallationApp.DestinationPositionRequired");
                                    this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                    return this.currentError;
                                }

                                if (this.DestinationPosition2.Value < 0)
                                {
                                    this.currentError = Localized.Get("InstallationApp.DestinationPositionMustBePositive");
                                    this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                    return this.currentError;
                                }

                                if (this.DestinationPosition2.Value < this.axisLowerBound ||
                                    this.DestinationPosition2.Value > this.axisUpperBound)
                                {
                                    this.currentError = string.Format(Localized.Get("InstallationApp.DestinationPositionOutOfRangeAxis"), this.AxisLowerBound, this.AxisUpperBound);
                                    this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                    return this.currentError;
                                }
                            }

                            if (this.MeasuredPosition2.HasValue &&
                                (this.MeasuredPosition2.Value < this.axisLowerBound ||
                                 this.MeasuredPosition2.Value > this.axisUpperBound) &&
                                Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) == Convert.ToInt32(this.DestinationPosition2.Value))
                            {
                                this.currentError = string.Format(Localized.Get("InstallationApp.MeasuredPositionOutOfRangeAxis"), this.AxisLowerBound, this.AxisUpperBound);
                                this.ShowNotification(this.currentError, NotificationSeverity.Warning);
                                return this.currentError;
                            }
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

                if (this.AxisUpperBound == 0 || this.AxisLowerBound == 0 || this.StartPosition == 0 || !this.DestinationPosition1.HasValue || !this.DestinationPosition2.HasValue)
                {
                    var procedureParameters = await this.verticalOriginProcedureWebService.GetParametersAsync();
                    this.ProcedureParameters = await this.resolutionCalibrationWebService.GetParametersAsync();

                    this.StartPosition = this.ProcedureParameters.StartPosition;
                    this.DestinationPosition1 = this.ProcedureParameters.InitialPosition;
                    this.DestinationPosition2 = this.ProcedureParameters.FinalPosition;
                    this.AxisUpperBound = procedureParameters.UpperBound;
                    this.AxisLowerBound = procedureParameters.LowerBound;
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
                case CalibrationStep.PositionMeter:
                    if (e.Next)
                    {
                        this.CurrentStep = CalibrationStep.FirstMeasured;
                    }

                    break;

                case CalibrationStep.FirstMeasured:
                    if (e.Next)
                    {
                        this.CurrentStep = CalibrationStep.LastMeasured;
                    }
                    else
                    {
                        this.CurrentStep = CalibrationStep.PositionMeter;
                    }

                    break;

                case CalibrationStep.LastMeasured:
                    if (e.Next)
                    {
                        this.RetrieveNewResolutionAsync();
                        this.CurrentStep = CalibrationStep.Confirm;
                    }
                    else
                    {
                        this.CurrentStep = CalibrationStep.FirstMeasured;
                    }

                    break;

                case CalibrationStep.Confirm:
                    if (!e.Next)
                    {
                        this.CurrentStep = CalibrationStep.LastMeasured;
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
            this.RaisePropertyChanged(nameof(this.IsCanDestinationPosition1));
            this.RaisePropertyChanged(nameof(this.IsCanDestinationPosition2));
            this.RaisePropertyChanged(nameof(this.IsCanMeasuredPosition1));
            this.RaisePropertyChanged(nameof(this.IsCanMeasuredPosition2));

            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToStartPositionCommand?.RaiseCanExecuteChanged();

            this.moveToStartDestination1Command?.RaiseCanExecuteChanged();
            this.moveToStartDestination2Command?.RaiseCanExecuteChanged();

            this.moveToFirstMeasuredCommand?.RaiseCanExecuteChanged();
            this.moveToLastMeasuredCommand?.RaiseCanExecuteChanged();
            this.moveToConfirmCommand?.RaiseCanExecuteChanged();
            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.UpdateVerticalResolutionAsync(this.NewResolution.Value);

                this.CurrentResolution = this.NewResolution;
                this.MeasuredPosition1 = null;
                this.MeasuredPosition2 = null;
                this.CurrentStep = CalibrationStep.PositionMeter;

                this.ShowNotification(
                    VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                    Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
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

        private bool CanMoveToStartDestination1()
        {
            return this.CanBaseExecute() &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.DestinationPosition1.GetValueOrDefault(0));
        }

        private bool CanMoveToStartDestination2()
        {
            return this.CanBaseExecute() &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.DestinationPosition2.GetValueOrDefault(0));
        }

        private bool CanMoveToStartPosition()
        {
            var b = this.CanBaseExecute() &&
                   string.IsNullOrEmpty(this.Error) &&
                   Convert.ToInt32(this.MachineStatus.ElevatorVerticalPosition.Value) != Convert.ToInt32(this.StartPosition);
            return b;
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanToConfirm()
        {
            return this.CanBaseExecute() &&
                   this.MeasuredPosition2.HasValue;
        }

        private bool CanToFirstMeasured()
        {
            return this.CanBaseExecute();
        }

        private bool CanToLastMeasured()
        {
            return this.CanBaseExecute() &&
                   this.MeasuredPosition1.HasValue;
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.luPresentInOperatorSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
            this.luPresentInMachineSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
        }

        private async void RetrieveNewResolutionAsync()
        {
            if (this.MeasuredPosition1.HasValue &&
                this.MeasuredPosition2.HasValue)
            {
                this.MeasuredDistance = this.MeasuredPosition2 - this.MeasuredPosition1;

                this.tokenSource?.Cancel(false);
                this.tokenSource = new CancellationTokenSource();

                try
                {
                    const int callDelayMilliseconds = 300;

                    await Task
                        .Delay(callDelayMilliseconds, this.tokenSource.Token)
                        .ContinueWith(
                            async t => await this.RetrieveNewResolutionAsync(this.tokenSource.Token),
                            this.tokenSource.Token,
                            TaskContinuationOptions.NotOnCanceled,
                            TaskScheduler.Current)
                        .ConfigureAwait(true);
                }
                catch (TaskCanceledException)
                {
                    this.IsRetrievingNewResolution = false;
                }
            }
        }

        private async Task RetrieveNewResolutionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var exepectedDistance = this.DestinationPosition2.Value - this.DestinationPosition1.Value;

                this.IsRetrievingNewResolution = true;

                this.NewResolution = await this.resolutionCalibrationWebService
                    .GetAdjustedResolutionAsync(
                        this.MeasuredDistance.Value,
                        exepectedDistance,
                        cancellationToken);

                if (Math.Abs(this.newResolution.Value - this.currentResolution.Value) > 0.06)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        var message = Localized.Get("InstallationApp.ResultOutOfStandard")
                        + "\r\n \u25E6 " + Localized.Get("InstallationApp.UseMachineFullStroke")
                        + "\r\n \u25E6 " + Localized.Get("InstallationApp.AttachMeterToElevator")
                        + "\r\n \u25E6 " + Localized.Get("InstallationApp.MetricCordPerpendicular")
                        + "\r\n \u25E6 " + Localized.Get("InstallationApp.UseLaserLevelToGetValue")
                        + "\r\n " + Localized.Get("InstallationApp.WantRepeatProcedure");
                        var messageBoxResult = this.dialogService.ShowMessage(message, Localized.Get("OperatorApp.Warning"), DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult is DialogResult.Yes)
                        {
                            this.MeasuredPosition1 = null;
                            this.MeasuredPosition2 = null;
                            this.CurrentStep = CalibrationStep.PositionMeter;
                        }
                    });
                }

                this.IsRetrievingNewResolution = false;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.NewResolution = null;
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

            this.themeChangedToken = this.themeChangedToken
                ?? this.EventAggregator
                    .GetEvent<ThemeChangedPubSubEvent>()
                    .Subscribe(
                        (m) =>
                        {
                            this.RaisePropertyChanged(nameof(this.HasStepPositionMeter));
                            this.RaisePropertyChanged(nameof(this.HasStepFirstMeasured));
                            this.RaisePropertyChanged(nameof(this.HasStepLastMeasured));
                            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
                        },
                        ThreadOption.UIThread,
                        false);
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case CalibrationStep.PositionMeter:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, this.moveToFirstMeasuredCommand?.CanExecute() ?? false);
                    break;

                case CalibrationStep.FirstMeasured:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToLastMeasuredCommand?.CanExecute() ?? false);
                    break;

                case CalibrationStep.LastMeasured:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, this.moveToConfirmCommand?.CanExecute() ?? false);
                    break;

                case CalibrationStep.Confirm:
                    this.ShowPrevStepSinglePage(true, !this.IsMoving);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepPositionMeter));
            this.RaisePropertyChanged(nameof(this.HasStepFirstMeasured));
            this.RaisePropertyChanged(nameof(this.HasStepLastMeasured));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
        }

        #endregion
    }
}
