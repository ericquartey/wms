using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ElevatorWeightAnalysisViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly ObservableCollection<DataSample> measuredSamples = new ObservableCollection<DataSample>();

        private readonly List<DataSample> measuredSamplesInCurrentSession = new List<DataSample>();

        private readonly IMachineWeightAnalysisProcedureService weightAnalysisProcedureService;

        private decimal? averageCurrent;

        private bool canInputNetWeight;

        private decimal? currentPosition;

        private decimal? inputDisplacement;

        private string inputLoadingUnitCode;

        private decimal? inputNetWeight;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private LoadingUnit loadingUnit;

        private IEnumerable<LoadingUnit> loadingUnits;

        private DelegateCommand moveToBayCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public ElevatorWeightAnalysisViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorService machineElevatorService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IMachineWeightAnalysisProcedureService weightAnalysisProcedureService,
            IBayManager bayManager)
            : base(Services.PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (machineLoadingUnitsService is null)
            {
                throw new ArgumentNullException(nameof(machineLoadingUnitsService));
            }

            if (weightAnalysisProcedureService is null)
            {
                throw new ArgumentNullException(nameof(weightAnalysisProcedureService));
            }

            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.eventAggregator = eventAggregator;
            this.machineElevatorService = machineElevatorService;
            this.machineLoadingUnitsService = machineLoadingUnitsService;
            this.weightAnalysisProcedureService = weightAnalysisProcedureService;
            this.bayManager = bayManager;
        }

        #endregion

        #region Properties

        public decimal? AverageCurrent
        {
            get => this.averageCurrent;
            private set => this.SetProperty(ref this.averageCurrent, value);
        }

        public bool CanInputNetWeight
        {
            get => this.canInputNetWeight;
            set => this.SetProperty(ref this.canInputNetWeight, value);
        }

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        public string Error => string.Join(
          Environment.NewLine,
          this[nameof(this.InputDisplacement)]);

        public decimal? InputDisplacement
        {
            get => this.inputDisplacement;
            set
            {
                if (this.SetProperty(ref this.inputDisplacement, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string InputLoadingUnitCode
        {
            get => this.inputLoadingUnitCode;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitCode, value))
                {
                    if (value != null)
                    {
                        this.LoadingUnit = this.loadingUnits.SingleOrDefault(l => l.Code == value);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal? InputNetWeight
        {
            get => this.inputNetWeight;
            set
            {
                if (this.SetProperty(ref this.inputNetWeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    if (this.isExecutingProcedure)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
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

        public LoadingUnit LoadingUnit
        {
            get => this.loadingUnit;
            set
            {
                if (this.SetProperty(ref this.loadingUnit, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<DataSample> MeasuredSamples => this.measuredSamples;

        public ICommand MoveToBayCommand =>
           this.moveToBayCommand
           ??
           (this.moveToBayCommand = new DelegateCommand(
               async () => await this.MoveToBayAsync(),
               this.CanMoveToBay));

        public ICommand StartCommand =>
                  this.startCommand
          ??
          (this.startCommand = new DelegateCommand(
              async () => await this.StartAsync(),
              this.CanStart));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputDisplacement):
                        if (!this.InputDisplacement.HasValue)
                        {
                            return $"InputDisplacement is required.";
                        }

                        if (this.InputDisplacement.Value <= 0)
                        {
                            return "InputDisplacement must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputNetWeight):
                        if (!this.InputNetWeight.HasValue)
                        {
                            return $"InputNetWeight is required.";
                        }

                        if (this.InputNetWeight.Value <= 0)
                        {
                            return "InputNetWeight must be strictly positive.";
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

            if (this.subscriptionToken != null)
            {
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;
            this.subscriptionToken = this.eventAggregator
             .GetEvent<NotificationEventUI<PositioningMessageData>>()
             .Subscribe(
                 message => this.OnAutomationMessageReceived(message),
                 ThreadOption.UIThread,
                 false);

            await this.RetrieveCurrentPositionAsync();

            await this.RetrieveLoadingUnitsAsync();
        }

        protected virtual void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            System.Diagnostics.Debug.WriteLine($"Positioning notification: mode={message?.Data?.MovementMode}, axis={message.Data.AxisMovement}, status={message?.Status}, position={message?.Data.CurrentPosition}, sample={message?.Data?.TorqueCurrentSample?.Value}");

            if (message is null
                || message.Data is null
                || message.Data.AxisMovement != Axis.Vertical)
            {
                return;
            }

            this.CurrentPosition = message.Data.CurrentPosition ?? this.CurrentPosition;

            if (message.Status == MessageStatus.OperationExecuting
                &&
                message.Data.TorqueCurrentSample != null)
            {
                this.measuredSamples.Add(message.Data.TorqueCurrentSample);
                this.measuredSamplesInCurrentSession.Add(message.Data.TorqueCurrentSample);
            }

            if (message.Status == MessageStatus.OperationStop)
            {
                this.IsExecutingProcedure = false;

                this.AverageCurrent = this.measuredSamplesInCurrentSession.Average(s => s.Value);

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }
            else if (message.Status == MessageStatus.OperationError)
            {
                this.IsExecutingProcedure = false;

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Error);
            }
            else if (message.Status == MessageStatus.OperationEnd)
            {
                this.IsExecutingProcedure = false;

                if (this.measuredSamplesInCurrentSession.Any())
                {
                    this.AverageCurrent = this.measuredSamplesInCurrentSession.Average(s => s.Value);
                }

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureCompleted,
                    Services.Models.NotificationSeverity.Success);
            }
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            this.stopCommand?.RaiseCanExecuteChanged();
            this.startCommand?.RaiseCanExecuteChanged();

            this.CanInputNetWeight = this.loadingUnit != null;
        }

        private bool CanMoveToBay()
        {
            return
               !this.IsExecutingProcedure
               &&
               !this.IsWaitingForResponse;
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
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private async Task MoveToBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                var bayHeight = this.bayManager.Bay.Positions.First();

                await this.machineElevatorService.MoveToVerticalPositionAsync(bayHeight, FeedRateCategory.VerticalManualMovementsAfterZero);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.CurrentPosition = await this.machineElevatorService.GetVerticalPositionAsync();
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

        private async Task RetrieveLoadingUnitsAsync()
        {
            this.loadingUnits = await this.machineLoadingUnitsService.GetAllAsync();
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                this.measuredSamplesInCurrentSession.Clear();
                this.AverageCurrent = null;

                await this.weightAnalysisProcedureService.StartAsync(
                    this.InputDisplacement.Value,
                    this.InputNetWeight.Value,
                    this.loadingUnit?.Id);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
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
                await this.machineElevatorService.StopAsync();
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

        #endregion

        #region Classes

        public class MeasuredSample
        {
            #region Properties

            public DateTime Time { get; set; }

            public double Value { get; set; }

            #endregion
        }

        #endregion
    }
}
