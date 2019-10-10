using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BeltBurnishingViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineBeltBurnishingProcedureService beltBurnishingService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineSetupStatusService machineSetupStatusService;

        private int? completedCycles;

        private double? currentPosition;

        private int initialCycles;

        private int inputDelay;

        private double? inputLowerBound;

        private int? inputRequiredCycles;

        private double? inputUpperBound;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private double? machineLowerBound;

        private double? machineUpperBound;

        private SubscriptionToken receivedActionUpdateToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private int? totalCompletedCycles;

        #endregion

        #region Constructors

        public BeltBurnishingViewModel(
            IEventAggregator eventAggregator,
            IMachineSetupStatusService machineSetupStatusService,
            IMachineBeltBurnishingProcedureService beltBurnishingService)
            : base(Services.PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (beltBurnishingService is null)
            {
                throw new ArgumentNullException(nameof(beltBurnishingService));
            }

            this.eventAggregator = eventAggregator;
            this.machineSetupStatusService = machineSetupStatusService;
            this.beltBurnishingService = beltBurnishingService;
            this.inputDelay = 0;
        }

        #endregion

        #region Properties

        public int? CompletedCycles
        {
            get => this.completedCycles;
            private set => this.SetProperty(ref this.completedCycles, value);
        }

        public double? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

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

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        public int? TotalCompletedCycles
        {
            get => this.totalCompletedCycles;
            private set => this.SetProperty(ref this.totalCompletedCycles, value);
        }

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

            if (this.receivedActionUpdateToken != null)
            {
                this.eventAggregator
                  .GetEvent<NotificationEventUI<PositioningMessageData>>()
                  .Unsubscribe(this.receivedActionUpdateToken);

                this.receivedActionUpdateToken = null;
            }
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                var procedureParameters = await this.beltBurnishingService.GetParametersAsync();

                this.InputUpperBound = procedureParameters.UpperBound;
                this.machineUpperBound = procedureParameters.UpperBound;
                this.InputLowerBound = procedureParameters.LowerBound;
                this.machineLowerBound = procedureParameters.LowerBound;

                this.InputRequiredCycles = procedureParameters.RequiredCycles;

                await this.InitializeTotalCycles();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.CompletedCycles = 0;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            await this.GetParameterValuesAsync();

            this.receivedActionUpdateToken = this.eventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    async message => await this.UpdateCompletion(message),
                    ThreadOption.UIThread,
                    false);
        }

        private bool CanExecuteStartCommand()
        {
            return !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error)
                && this.machineLowerBound <= this.inputLowerBound
                && this.machineUpperBound >= this.inputUpperBound;
        }

        private bool CanExecuteStopCommand()
        {
            return this.IsExecutingProcedure
                && !this.IsWaitingForResponse;
        }

        private async Task InitializeTotalCycles()
        {
            var setupStatus = await this.machineSetupStatusService.GetAsync();
            this.initialCycles = setupStatus.BeltBurnishing.CompletedCycles;
            this.TotalCompletedCycles = this.initialCycles;
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            try
            {
                if ((this.InputRequiredCycles.Value - this.TotalCompletedCycles.Value) <= 0)
                {
                    this.ShowNotification("Total completed cycles are greater than required cycles.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.beltBurnishingService.StartAsync(
                    this.InputUpperBound.Value,
                    this.InputLowerBound.Value,
                    this.InputRequiredCycles.Value - this.TotalCompletedCycles.Value,
                    this.InputDelay);

                await this.InitializeTotalCycles();
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
            try
            {
                this.IsWaitingForResponse = true;

                await this.beltBurnishingService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsExecutingProcedure = false;
            }
        }

        private async Task UpdateCompletion(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null)
            {
                return;
            }

            this.TotalCompletedCycles = this.initialCycles + message.Data.ExecutedCycles;
            this.CompletedCycles = message.Data.ExecutedCycles;
            this.CurrentPosition = message?.Data?.CurrentPosition ?? this.CurrentPosition;

            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    break;

                case MessageStatus.OperationEnd:
                    this.IsExecutingProcedure = false;

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    this.IsExecutingProcedure = false;
                    this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped);
                    break;

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;
                    break;
            }
        }

        #endregion
    }
}
