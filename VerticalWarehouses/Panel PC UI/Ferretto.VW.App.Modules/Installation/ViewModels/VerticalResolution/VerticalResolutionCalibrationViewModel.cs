using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal class VerticalResolutionCalibrationViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService;

        private decimal? currentResolution;

        private double? inputInitialPosition;

        private bool isExecutingProcedure;

        private bool isLoadingUnitOnBoard;

        private bool isOperationCompleted;

        private LoadingUnit loadingUnitOnBoard;

        private bool? luPresentInMachineSide;

        private bool? luPresentInOperatorSide;

        private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken sensorsToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IMachineElevatorService machineElevatorService)
          : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.MachineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
        }

        #endregion

        #region Properties

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputInitialPosition)]);

        public double? InputInitialPosition
        {
            get => this.inputInitialPosition;
            set => this.SetProperty(ref this.inputInitialPosition, value, this.RaiseCanExecuteChanged);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set => this.SetProperty(ref this.isExecutingProcedure, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLoadingUnitOnBoard
        {
            get => this.isLoadingUnitOnBoard;
            private set => this.SetProperty(ref this.isLoadingUnitOnBoard, value);
        }

        public override bool IsWaitingForResponse
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

        public IMachineElevatorWebService MachineElevatorWebService { get; }

        public IMachineVerticalResolutionCalibrationProcedureWebService ResolutionCalibrationService => this.resolutionCalibrationWebService;

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

        internal IEventAggregator EventAggregator => this.eventAggregator;

        internal IHealthProbeService HealthProbeService => this.healthProbeService;

        protected VerticalResolutionCalibrationProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputInitialPosition):
                        if (!this.InputInitialPosition.HasValue)
                        {
                            return $"InputInitialPosition is required.";
                        }

                        if (this.InputInitialPosition.Value <= 0)
                        {
                            return "InputInitialPosition must be strictly positive.";
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

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
        }

        public async Task GetParametersAsync()
        {
            try
            {
                this.CurrentResolution = await this.MachineElevatorWebService.GetVerticalResolutionAsync();

                this.InputInitialPosition = this.ProcedureParameters.InitialPosition;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningOperationChanged,
                        ThreadOption.UIThread,
                        false);

            await this.RetrieveProcedureParametersAsync();

            await this.GetParametersAsync();

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

            this.IsBackNavigationAllowed = true;
        }

        protected void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.IsErrored())
            {
                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }

            if (message.Data?.AxisMovement != CommonUtils.Messages.Enumerations.Axis.Vertical)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.isOperationCompleted = true;
                this.NavigateToNextStep();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
            this.startCommand?.RaiseCanExecuteChanged();
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

        private async Task GetSensorsAndLoadingUnitOnBoardAsync()
        {
            if (this.HealthProbeService.HealthStatus != HealthStatus.Healthy)
            {
                return;
            }

            try
            {
                this.loadingUnitOnBoard = await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync();
                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                var sensors = new Sensors();
                sensors.Update(sensorsStates.ToArray());

                this.SetIsLoadingUnitOnBord(sensors);
            }
            catch
            {
                throw;
            }
        }

        private void NavigateToNextStep()
        {
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION))
            {
                var wizardData = new VerticalResolutionWizardData
                {
                    CurrentResolution = this.CurrentResolution.Value,
                    FinalPosition = this.ProcedureParameters.FinalPosition,
                    InitialPosition = this.InputInitialPosition.Value,
                };

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION,
                    wizardData,
                    trackCurrentView: false);
            }
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.luPresentInOperatorSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
            this.luPresentInMachineSide = message?.Data.SensorsStates[(int)IOMachineSensors.RunningState];
        }

        private async Task RetrieveProcedureParametersAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ProcedureParameters = await this.resolutionCalibrationWebService.GetParametersAsync();
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

        private void SetIsLoadingUnitOnBord(Sensors sensors)
        {
            this.IsLoadingUnitOnBoard = sensors.LuPresentInMachineSide || sensors.LuPresentInOperatorSide;
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, false);
            this.ShowNextStep(true, this.isOperationCompleted, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VERTICALRESOLUTIONCALIBRATION);
            this.ShowAbortStep(true, true);
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.MachineElevatorWebService.MoveManualToVerticalPositionAsync(
                    this.InputInitialPosition.Value,
                    false,
                    false);
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
                await this.MachineElevatorWebService.StopAsync();
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
    }
}
