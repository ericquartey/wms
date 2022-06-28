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
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BeltBurnishingViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        // TODO: Move this to parameters
        private const int positionOffset = 500; //mm

        private readonly IMachineBeltBurnishingProcedureWebService beltBurnishingWebService;

        private readonly Services.IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly ISessionService sessionService;

        private int? completedCyclesThisSession;

        private double? currentPosition;

        private double? cyclesPercent;

        private SubscriptionToken elevatorPositionChangedToken;

        private int inputDelay;

        private double? inputLowerBound;

        private int? inputRequiredCycles;

        private double? inputUpperBound;

        private bool isBeltBurnishing = false;

        private bool isCarouselCalibration = false;

        //private bool isCompleted;

        private bool isExecutingProcedure;

        private double? machineLowerBound;

        private double? machineUpperBound;

        private SubscriptionToken positioningMessageReceivedToken;

        private DelegateCommand resetCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private int? totalCompletedCycles;

        private int totalPerformedCyclesBeforeStart;

        #endregion

        #region Constructors

        public BeltBurnishingViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBeltBurnishingProcedureWebService beltBurnishingWebService,
            IMachineElevatorService machineElevatorService,
            ISessionService sessionService,
            IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.beltBurnishingWebService = beltBurnishingWebService ?? throw new ArgumentNullException(nameof(beltBurnishingWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public int? CumulativePerformedCycles
        {
            get => this.totalCompletedCycles;
            private set
            {
                if (this.SetProperty(ref this.totalCompletedCycles, value))
                {
                    this.PerformedCyclesThisSession = value.Value - this.totalPerformedCyclesBeforeStart;
                }
            }
        }

        public double? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            this[nameof(this.InputLowerBound)],
            this[nameof(this.InputUpperBound)],
            this[nameof(this.InputRequiredCycles)],
            this[nameof(this.InputDelay)]);

        public int InputDelay
        {
            get => this.inputDelay;
            set
            {
                if (this.SetProperty(ref this.inputDelay, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? InputLowerBound
        {
            get => this.inputLowerBound;
            set
            {
                if (this.SetProperty(ref this.inputLowerBound, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputRequiredCycles
        {
            get => this.inputRequiredCycles;
            set
            {
                if (this.SetProperty(ref this.inputRequiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? InputUpperBound
        {
            get => this.inputUpperBound;
            set
            {
                if (this.SetProperty(ref this.inputUpperBound, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsEnabledEditing => !this.IsExecutingProcedure && this.sessionService.UserAccessLevel is MAS.AutomationService.Contracts.UserAccessLevel.Admin;

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

        public int? PerformedCyclesThisSession
        {
            get => this.completedCyclesThisSession;
            private set => this.SetProperty(ref this.completedCyclesThisSession, value);
        }

        public ICommand ResetCommand =>
            this.resetCommand
            ??
            (this.resetCommand = new DelegateCommand(
                async () => await this.ResetAsync(),
                this.CanExecuteResetCommand));

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartTestAsync(),
                this.CanStartTest));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopTestAsync(),
                this.CanStopTest));

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
                    case nameof(this.InputDelay):
                        if (this.InputDelay < 0)
                        {
                            return Localized.Get("InstallationApp.InputDelayMustBePositive");
                        }

                        break;

                    case nameof(this.InputRequiredCycles):
                        if (!this.InputRequiredCycles.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputRequiredCyclesMustBePositive");
                        }

                        if (this.InputRequiredCycles.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.InputRequiredCyclesRequired");
                        }

                        break;

                    case nameof(this.InputLowerBound):
                        if (!this.InputLowerBound.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputLowerBoundRequired");
                        }

                        if (this.InputLowerBound.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.InputLowerBoundMustBePositive");
                        }

                        if (this.InputUpperBound.HasValue
                          &&
                          this.InputUpperBound.Value < this.InputLowerBound.Value)
                        {
                            return Localized.Get("InstallationApp.InputLowerBoundMustBeGreatherThanUpper");
                        }

                        if (this.InputLowerBound.Value < this.machineLowerBound)
                        {
                            return string.Format(Localized.Get("InstallationApp.InputLowerBoundMustBeGreatherThanValue"), this.machineLowerBound);
                        }

                        break;

                    case nameof(this.InputUpperBound):
                        if (!this.InputUpperBound.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputUpperBoundRequired");
                        }

                        if (this.InputUpperBound.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.InputUpperBoundMustBePositive");
                        }

                        if (this.InputLowerBound.HasValue
                            &&
                            this.InputUpperBound.Value < this.InputLowerBound.Value)
                        {
                            return Localized.Get("InstallationApp.InputUpperBoundMustBeGreatherThanLower");
                        }

                        if (this.InputUpperBound.Value > this.machineUpperBound)
                        {
                            return string.Format(Localized.Get("InstallationApp.InputUpperBoundMustBeGreatherThanValue"), this.machineUpperBound);
                        }

                        break;
                }

                if (this.IsVisible)
                {
                    this.ClearNotifications();
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
            /*
            if (this.positioningMessageReceivedToken != null)
            {
                this.EventAggregator?.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.positioningMessageReceivedToken);
                this.positioningMessageReceivedToken?.Dispose();
                this.positioningMessageReceivedToken = null;
            }

            if (this.elevatorPositionChangedToken != null)
            {
                this.EventAggregator?.GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>().Unsubscribe(this.elevatorPositionChangedToken);
                this.elevatorPositionChangedToken?.Dispose();
                this.elevatorPositionChangedToken = null;
            }
            */
        }

        public async Task GetParameterValuesAsync()
        {
            if (this.InputRequiredCycles == null || this.CumulativePerformedCycles == null)
            {
                var procedureParameters = await this.beltBurnishingWebService.GetParametersAsync();
                this.InputRequiredCycles = procedureParameters.RequiredCycles;
                this.CumulativePerformedCycles = procedureParameters.PerformedCycles;
            }

            if (this.InputUpperBound == null || this.machineUpperBound == null || this.InputLowerBound == null || this.machineLowerBound == null)
            {
                var bounds = await this.machineElevatorWebService.GetVerticalBoundsAsync();

                this.InputUpperBound = bounds.Upper - positionOffset;
                this.machineUpperBound = bounds.Upper;
                this.InputLowerBound = bounds.Lower + positionOffset;
                this.machineLowerBound = bounds.Lower;
            }
        }

        public override async Task OnAppearedAsync()
        {
            if (this.MachineService.MachineMode == MachineMode.Test ||
                this.MachineService.MachineMode == MachineMode.Test2 ||
                this.MachineService.MachineMode == MachineMode.Test3)
            {
                this.IsExecutingProcedure = true;
                this.PerformedCyclesThisSession = 0;
            }
            else
            {
                this.isBeltBurnishing = false;
            }

            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected async override Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                await this.GetParameterValuesAsync();

                this.IsExecutingProcedure = (this.MachineService.MachineStatus.IsMoving ||
                    this.MachineService.MachineMode == MachineMode.Test ||
                    this.MachineService.MachineMode == MachineMode.Test2 ||
                    this.MachineService.MachineMode == MachineMode.Test3);

                this.CurrentPosition = this.machineElevatorService.Position.Vertical;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState == MachinePowerState.Unpowered)
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
                this.RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.resetCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
        }

        private bool CanExecuteResetCommand()
        {
            return this.CumulativePerformedCycles.HasValue &&
                   this.CumulativePerformedCycles > 0 &&
                   !this.MachineService.MachineStatus.IsMoving &&
                   !this.IsExecutingProcedure &&
                   string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanStartTest()
        {
            return
                (!this.MachineService.MachineStatus.IsMoving || this.isCarouselCalibration)
                &&
                (this.SensorsService.ShutterSensors.Closed || !this.MachineService.HasShutter ||
                (this.SensorsService.ShutterSensors.MidWay && this.MachineService.Bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.UpperHalf))
                //&&
                //(!this.IsExecutingProcedure || this.isCarouselCalibration)
                &&
                !this.isBeltBurnishing
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanStopTest()
        {
            return this.IsMoving || this.IsExecutingProcedure;
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.CurrentPosition = e.VerticalPosition;
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Data?.MovementMode == MovementMode.BayTest)
            {
                this.isCarouselCalibration = true;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored())
            {
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), Services.Models.NotificationSeverity.Warning);
                this.IsExecutingProcedure = false;
            }

            if (message.Data?.MovementMode == MovementMode.BeltBurnishing)
            {
                this.isBeltBurnishing = true;
                if (this.PerformedCyclesThisSession == null && this.CumulativePerformedCycles.HasValue)
                {
                    this.totalPerformedCyclesBeforeStart = this.CumulativePerformedCycles.Value;
                }

                this.CumulativePerformedCycles = message.Data.ExecutedCycles;
                if (this.InputRequiredCycles.HasValue)
                {
                    this.CyclesPercent = ((double)(this.CumulativePerformedCycles ?? 0) / (double)this.InputRequiredCycles) * 100.0;
                }
                else
                {
                    this.CyclesPercent = null;
                }
            }

            if (message.Status == MessageStatus.OperationEnd &&
                message.Data?.MovementMode == MovementMode.BeltBurnishing &&
                message.Data?.ExecutedCycles == message.Data.RequiredCycles)
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.BeltMustBeTight"), Localized.Get("InstallationApp.BeltBurnishing"), DialogType.Exclamation, DialogButtons.OK);
                if (messageBoxResult == DialogResult.OK)
                {
                    ;
                }

                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.CompletedTest"), Services.Models.NotificationSeverity.Success);
                //this.isCompleted = true;
                this.IsExecutingProcedure = false;
            }
        }

        private async Task ResetAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ResetTotalCyclesNumber"), Localized.Get("InstallationApp.BeltBreakIn"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.CumulativePerformedCycles = 0;
                    this.PerformedCyclesThisSession = 0;
                    this.CyclesPercent = 0;

                    await this.beltBurnishingWebService.ResetAsync();
                }
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StartTestAsync()
        {
            try
            {
                var totalCyclesToPerform = this.InputRequiredCycles.Value - this.CumulativePerformedCycles.Value;
                if (totalCyclesToPerform <= 0)
                {
                    //this.isCompleted = true;
                    this.ShowNotification(Localized.Get("InstallationApp.RequiredCyclesCompleted"), Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                this.totalPerformedCyclesBeforeStart = this.CumulativePerformedCycles ?? 0;
                this.PerformedCyclesThisSession = 0;
                this.RaisePropertyChanged(nameof(this.PerformedCyclesThisSession));

                //this.isCompleted = false;

                await this.beltBurnishingWebService.StartAsync(
                    this.InputUpperBound.Value,
                    this.InputLowerBound.Value,
                    this.InputDelay);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopTestAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.MachineService.StopMovingByAllAsync();

                //this.isCompleted = true;
            }
            catch (Exception ex)
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
            this.positioningMessageReceivedToken = this.positioningMessageReceivedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningMessageReceived,
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
