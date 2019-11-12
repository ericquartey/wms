using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class BeltBurnishingViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineBeltBurnishingProcedureWebService beltBurnishingWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private int? completedCyclesThisSession;

        private double? currentPosition;

        private SubscriptionToken elevatorPositionChangedToken;

        private int inputDelay;

        private double? inputLowerBound;

        private int? inputRequiredCycles;

        private double? inputUpperBound;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private double? machineLowerBound;

        private double? machineUpperBound;

        private SubscriptionToken positioningMessageReceivedToken;

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
            IMachineElevatorService machineElevatorService)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.beltBurnishingWebService = beltBurnishingWebService ?? throw new ArgumentNullException(nameof(beltBurnishingWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
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

        public override EnableMask EnableMask => EnableMask.MachineManualMode;

        public string Error => string.Join(
            Environment.NewLine,
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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? PerformedCyclesThisSession
        {
            get => this.completedCyclesThisSession;
            private set => this.SetProperty(ref this.completedCyclesThisSession, value);
        }

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
                switch (columnName)
                {
                    case nameof(this.InputDelay):
                        if (this.InputDelay < 0)
                        {
                            return "InputDelay must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputRequiredCycles):
                        if (!this.InputRequiredCycles.HasValue)
                        {
                            return $"InputRequiredCycles is required.";
                        }

                        if (this.InputRequiredCycles.Value <= 0)
                        {
                            return "InputRequiredCycles must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputLowerBound):
                        if (!this.InputLowerBound.HasValue)
                        {
                            return $"InputLowerBound is required.";
                        }

                        if (this.InputLowerBound.Value <= 0)
                        {
                            return "InputLowerBound must be strictly positive.";
                        }

                        if (this.InputUpperBound.HasValue
                          &&
                          this.InputUpperBound.Value < this.InputLowerBound.Value)
                        {
                            return "InputLowerBound must be greater than InputUpperBound.";
                        }

                        if (this.InputLowerBound.Value < this.machineLowerBound)
                        {
                            return $"InputLowerBound must be greater than {this.machineLowerBound}.";
                        }

                        break;

                    case nameof(this.InputUpperBound):
                        if (!this.InputUpperBound.HasValue)
                        {
                            return $"InputUpperBound is required.";
                        }

                        if (this.InputUpperBound.Value <= 0)
                        {
                            return "InputLowerBound must be strictly positive.";
                        }

                        if (this.InputLowerBound.HasValue
                            &&
                            this.InputUpperBound.Value < this.InputLowerBound.Value)
                        {
                            return "InputUpperBound must be greater than InputLowerBound.";
                        }

                        if (this.InputUpperBound.Value > this.machineUpperBound)
                        {
                            return $"InputUpperBound must be greater than {this.machineUpperBound}.";
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

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.positioningMessageReceivedToken?.Dispose();
            this.positioningMessageReceivedToken = null;
            */
        }

        public async Task GetParameterValuesAsync()
        {
            var procedureParameters = await this.beltBurnishingWebService.GetParametersAsync();

            var bounds = await this.machineElevatorWebService.GetVerticalBoundsAsync();

            this.InputUpperBound = bounds.Upper;
            this.machineUpperBound = bounds.Upper;
            this.InputLowerBound = bounds.Lower;
            this.machineLowerBound = bounds.Lower;
            
            this.InputRequiredCycles = procedureParameters.RequiredCycles;            
            this.CumulativePerformedCycles = procedureParameters.PerformedCycles;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            try
            {
                this.IsWaitingForResponse = true;

                await this.GetParameterValuesAsync();

                this.CurrentPosition = this.machineElevatorService.Position.Vertical;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

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

        private bool CanStartTest()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanStopTest()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.CurrentPosition = e.VerticalPosition;
        }

        private void OnPositioningMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.IsErrored())
            {
                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, Services.Models.NotificationSeverity.Warning);
            }

            if (message.Data?.MovementMode == MovementMode.BeltBurnishing)
            {
                this.CumulativePerformedCycles = message.Data.ExecutedCycles;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartTestAsync()
        {
            try
            {
                var totalCyclesToPerform = this.InputRequiredCycles.Value - this.CumulativePerformedCycles.Value;
                if (totalCyclesToPerform <= 0)
                {
                    this.ShowNotification("Required amount of cycles was completed.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                this.totalPerformedCyclesBeforeStart = this.CumulativePerformedCycles ?? 0;
                this.PerformedCyclesThisSession = 0;
                this.RaisePropertyChanged(nameof(this.PerformedCyclesThisSession));

                await this.beltBurnishingWebService.StartAsync(
                    this.InputUpperBound.Value,
                    this.InputLowerBound.Value,
                    this.InputDelay);
            }
            catch (Exception ex)
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

                await this.beltBurnishingWebService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsExecutingProcedure = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
