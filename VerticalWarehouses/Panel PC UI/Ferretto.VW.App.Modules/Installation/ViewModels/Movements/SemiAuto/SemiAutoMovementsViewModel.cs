using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly IMachineSensorsService machineSensorsService;

        private readonly Sensors sensors = new Sensors();

        private readonly IMachineShuttersService shuttersService;

        private int? inputLoadingUnitCode;

        private bool isWaitingForResponse;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken sensorsToken;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public SemiAutoMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IMachineCellsService machineCellsService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IMachineSensorsService machineSensorsService,
            IMachineShuttersService shuttersService,
            IMachineServiceService machineServiceService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineCellsService is null)
            {
                throw new ArgumentNullException(nameof(machineCellsService));
            }

            if (machineLoadingUnitsService is null)
            {
                throw new ArgumentNullException(nameof(machineLoadingUnitsService));
            }

            if (bayManagerService is null)
            {
                throw new ArgumentNullException(nameof(bayManagerService));
            }

            if (machineSensorsService is null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            if (shuttersService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersService));
            }

            if (machineServiceService is null)
            {
                throw new System.ArgumentNullException(nameof(machineServiceService));
            }

            this.machineSensorsService = machineSensorsService;
            this.machineElevatorService = machineElevatorService;
            this.machineCellsService = machineCellsService;
            this.machineLoadingUnitsService = machineLoadingUnitsService;
            this.bayManagerService = bayManagerService;
            this.shuttersService = shuttersService;
            this.machineServiceService = machineServiceService;

            this.shutterSensors = new ShutterSensors(this.BayNumber);

            this.SelectBayPosition1();
        }

        #endregion

        #region Properties

        public int? InputLoadingUnitCode
        {
            get => this.inputLoadingUnitCode;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitCode, value)
                    &&
                    this.LoadingUnits != null)
                {
                    this.LoadingUnitInBay = value == null
                        ? null
                        : this.LoadingUnits.SingleOrDefault(l => l.Id == value);
                }
            }
        }

        public bool IsElevatorMoving => this.IsElevatorMovingToCell
                || this.IsElevatorMovingToHeight
                || this.IsElevatorMovingToLoadingUnit
                || this.IsElevatorMovingToBay
                || this.IsElevatorDisembarking
                || this.IsElevatorEmbarking
                || this.IsTuningChain;

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

        public IEnumerable<LoadingUnit> LoadingUnits { get => this.loadingUnits; set => this.loadingUnits = value; }

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }

            if (this.sensorsToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.sensorsToken);

                this.sensorsToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  message => this.OnElevatorPositionChanged(message),
                  ThreadOption.UIThread,
                  false);

            this.sensorsToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message =>
                        {
                            this.sensors.Update(message?.Data?.SensorsStates);
                            this.shutterSensors.Update(message?.Data?.SensorsStates);
                            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
                            this.RaiseCanExecuteChanged();
                        },
                    ThreadOption.UIThread,
                    false);
            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
                this.shutterSensors.Update(sensorsStates.ToArray());
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
            this.RaiseCanExecuteChanged();

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveCellsAsync();

            await this.RetrieveLoadingUnitsAsync();

            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.RetrieveElevatorPositionAsync();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsService.GetAllAsync();
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

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            // reset all status if stop machine
            if (e.MachinePower == Services.Models.MachinePowerState.Unpowered)
            {
                this.IsElevatorMovingToCell = false;
                this.IsElevatorMovingToHeight = false;
                this.IsElevatorMovingToLoadingUnit = false;
                this.IsElevatorMovingToBay = false;
                this.IsElevatorDisembarking = false;
                this.IsElevatorEmbarking = false;
                this.IsTuningChain = false;
            }
        }

        private void OnElevatorPositionChanged(CommonUtils.Messages.NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
            }

            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
                    {
                        if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message.Data.CurrentPosition;
                        }
                        else if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message.Data.CurrentPosition;
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToCell = false;
                        this.IsElevatorMovingToHeight = false;
                        this.IsElevatorMovingToLoadingUnit = false;
                        this.IsElevatorMovingToBay = false;
                        this.IsTuningChain = false;

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsElevatorDisembarking = false;
                        this.IsElevatorEmbarking = false;
                        this.IsElevatorMovingToCell = false;
                        this.IsElevatorMovingToHeight = false;
                        this.IsElevatorMovingToLoadingUnit = false;
                        this.IsElevatorMovingToBay = false;
                        this.IsTuningChain = false;

                        this.ShowNotification(
                            VW.App.Resources.InstallationApp.ProcedureWasStopped,
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.CanInputCellId = this.Cells != null
               &&
               !this.IsElevatorMoving
               &&
               !this.IsWaitingForResponse;

            this.CanInputQuote = !this.IsElevatorMoving
               &&
               !this.IsWaitingForResponse;

            this.CanInputLoadingUnitId = this.LoadingUnits != null
               &&
               this.Cells != null
               &&
               !this.IsElevatorMoving
               &&
               !this.IsWaitingForResponse;

            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
            this.moveToHeightCommand?.RaiseCanExecuteChanged();
            this.moveToLoadingUnitHeightCommand?.RaiseCanExecuteChanged();
            this.tuningBayCommand?.RaiseCanExecuteChanged();
            this.tuningChainCommand?.RaiseCanExecuteChanged();
            this.embarkForwardsCommand?.RaiseCanExecuteChanged();
            this.embarkBackwardsCommand?.RaiseCanExecuteChanged();
            this.disembarkForwardsCommand?.RaiseCanExecuteChanged();
            this.disembarkBackwardsCommand?.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.openShutterCommand?.RaiseCanExecuteChanged();
            this.intermediateShutterCommand?.RaiseCanExecuteChanged();
            this.closedShutterCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
