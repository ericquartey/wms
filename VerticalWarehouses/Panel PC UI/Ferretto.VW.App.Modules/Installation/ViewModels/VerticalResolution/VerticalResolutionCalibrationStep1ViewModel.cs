using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class VerticalResolutionCalibrationStep1ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private double? inputInitialPosition;

        private bool isLoadingUnitOnBoard;

        private bool isOperationCompleted;

        private LoadingUnit loadingUnitOnBoard;

        private bool? luPresentInMachineSide;

        private bool? luPresentInOperatorSide;

        private SubscriptionToken sensorsToken;

        private DelegateCommand startCommand;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep1ViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IMachineElevatorService machineElevatorService)
            : base(eventAggregator, machineElevatorWebService, resolutionCalibrationWebService, machineElevatorService, healthProbeService)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
        }

        #endregion

        #region Properties

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputInitialPosition)]);

        public double? InputInitialPosition
        {
            get => this.inputInitialPosition;
            set
            {
                if (this.SetProperty(ref this.inputInitialPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoadingUnitOnBoard
        {
            get => this.isLoadingUnitOnBoard;
            private set => this.SetProperty(ref this.isLoadingUnitOnBoard, value);
        }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanStart));

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
                            var res = !this.luPresentInOperatorSide.HasValue ||
                                      !this.luPresentInMachineSide.HasValue ||
                                      (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInOperatorSide] != this.luPresentInOperatorSide.Value) ||
                                      (m.Data.SensorsStates[(int)IOMachineSensors.LuPresentInMachineSide] != this.luPresentInMachineSide.Value);
                            this.luPresentInOperatorSide = m.Data.SensorsStates[(int)IOMachineSensors.RunningState];
                            this.luPresentInMachineSide = m.Data.SensorsStates[(int)IOMachineSensors.RunningState];
                            return res;
                        });
        }

        protected override void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnPositioningOperationChanged(message);

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.isOperationCompleted = true;
                this.NavigateToNextStep();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

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
            catch (Exception ex)
            {
                throw;
            }
        }

        private void NavigateToNextStep()
        {
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP1))
            {
                var wizardData = new VerticalResolutionWizardData
                {
                    CurrentResolution = this.CurrentResolution.Value,
                    FinalPosition = this.ProcedureParameters.FinalPosition,
                    InitialPosition = this.InputInitialPosition.Value,
                };

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.VerticalResolutionCalibration.STEP2,
                    wizardData,
                    trackCurrentView: false);
            }
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
        }

        private void SetIsLoadingUnitOnBord(Sensors sensors)
        {
            this.IsLoadingUnitOnBoard = sensors.LuPresentInMachineSide || sensors.LuPresentInOperatorSide;
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, false);
            this.ShowNextStep(true, this.isOperationCompleted, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP2);
            this.ShowAbortStep(true, true);
        }

        private async Task StartAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.MachineElevatorWebService.MoveToVerticalPositionAsync(
                    this.InputInitialPosition.Value,
                    this.ProcedureParameters.FeedRate,
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

        #endregion
    }
}
