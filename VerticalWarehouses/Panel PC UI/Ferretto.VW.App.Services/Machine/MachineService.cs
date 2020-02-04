using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Microsoft.AspNetCore.Http;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;
using ShutterType = Ferretto.VW.MAS.AutomationService.Contracts.ShutterType;

namespace Ferretto.VW.App.Services
{
    public sealed class MachineService : BindableBase, IMachineService, IDisposable
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly INavigationService navigationService;

        private readonly IRegionManager regionManager;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private Bay bay;

        private SubscriptionToken bayChainPositionChangedToken;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private IEnumerable<Bay> bays;

        private IEnumerable<Cell> cells;

        private SubscriptionToken elevatorPositionChangedToken;

        private bool executedInitialization;

        private SubscriptionToken fsmExceptionToken;

        private bool hasBayExternal;

        private bool hasCarousel;

        private bool hasShutter;

        private SubscriptionToken healthStatusChangedToken;

        private bool isDisposed;

        private bool isHoming;

        private bool isMissionInError;

        private bool isShutterThreeSensors;

        private IEnumerable<LoadingUnit> loadingUnits;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private MachineStatus machineStatus;

        private SubscriptionToken moveLoadingUnitToken;

        private string notification;

        private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private SubscriptionToken shutterPositionToken;

        #endregion

        #region Constructors

        public MachineService(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IMachineBaysWebService machineBaysWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineCellsWebService machineCellsWebService,
            IMachinePowerWebService machinePowerWebService,
            IMachineModeService machineModeService,
            ISensorsService sensorsService,
            IHealthProbeService healthProbeService,
            IBayManager bayManagerService,
            IMachineMissionsWebService missionsWebService)
        {
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machinePowerWebService = machinePowerWebService ?? throw new ArgumentNullException(nameof(machinePowerWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
        }

        #endregion

        #region Properties

        public Bay Bay
        {
            get => this.bay;
            private set => this.SetProperty(ref this.bay, value);
        }

        public bool BayFirstPositionIsUpper
        {
            get => this.bay?.Positions?.FirstOrDefault()?.IsUpper ?? false;
        }

        public MAS.AutomationService.Contracts.BayNumber BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value);
        }

        public IEnumerable<Cell> Cells
        {
            get => this.cells;
            set => this.SetProperty(ref this.cells, value, this.CellsNotificationProperty);
        }

        public bool HasBayExternal
        {
            get => this.hasBayExternal;
            set => this.SetProperty(ref this.hasBayExternal, value);
        }

        public bool HasCarousel
        {
            get => this.hasCarousel;
            set => this.SetProperty(ref this.hasCarousel, value);
        }

        public bool HasShutter
        {
            get => this.hasShutter;
            set => this.SetProperty(ref this.hasShutter, value);
        }

        public bool IsHoming
        {
            get => this.isHoming;
            set => this.SetProperty(ref this.isHoming, value);
        }

        public bool IsMissionInError
        {
            get => this.isMissionInError;
            private set => this.SetProperty(ref this.isMissionInError, value);
        }

        public bool IsShutterThreeSensors
        {
            get => this.isShutterThreeSensors;
            set => this.SetProperty(ref this.isShutterThreeSensors, value);
        }

        public IEnumerable<LoadingUnit> Loadunits
        {
            get => this.loadingUnits;
            set => this.SetProperty(ref this.loadingUnits, value, this.LoadUnitsNotificationProperty);
        }

        public MachineMode MachineMode => this.machineModeService.MachineMode;

        public MachinePowerState MachinePower => this.machineModeService.MachinePower;

        public MachineStatus MachineStatus
        {
            get => this.machineStatus;
            set => this.SetProperty(ref this.machineStatus, value, this.MachineStatusNotificationProperty);
        }

        internal string Notification
        {
            get => this.notification;
            set => this.SetProperty(ref this.notification, value, () => this.ShowNotification(this.notification, NotificationSeverity.Info));
        }

        #endregion

        #region Methods

        public void ClearNotifications()
        {
            this.notification = null;
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(true));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetBayPositionSourceByDestination(bool isPositionDownSelected)
        {
            if (this.bayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
                }
            }

            if (this.bayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
                }
            }

            if (this.bayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        public async Task OnInitializationServiceAsync()
        {
            if (this.healthProbeService.HealthMasStatus is HealthStatus.Healthy
                ||
                this.healthProbeService.HealthMasStatus is HealthStatus.Degraded)
            {
                try
                {
                    await this.InitializationHoming();
                    await this.InitializationBay();
                    await this.InitializationLoadUnits();
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
            this.executedInitialization = true;
        }

        public async Task OnUpdateServiceAsync()
        {
            if (!this.executedInitialization)
            {
                await this.OnInitializationServiceAsync();
            }
            else
            {
                await this.InitializationHoming();
                await this.UpdateBay();
                await this.InitializationLoadUnits();
                await this.machineModeService.OnUpdateServiceAsync();
            }
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.logger.Error(exception);

            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
        }

        public void Start()
        {
            this.machineStatus = new MachineStatus();
            this.loadingUnits = new List<LoadingUnit>();

            this.SubscribeToEvents();

            Task.Run(async () =>
            {
                try
                {
                    await this.OnInitializationServiceAsync();
                }
                catch (HttpRequestException)
                {
                }
                catch (Exception)
                {
                }
            }).GetAwaiter().GetResult();
        }

        public async Task StopMovingByAllAsync()
        {
            if (this.machineStatus.CurrentMissionId != null)
            {
                this.machineLoadingUnitsWebService?.StopAsync(this.machineStatus.CurrentMissionId, this.BayNumber);
            }

            this.machineElevatorWebService?.StopAsync();
            this.machineCarouselWebService?.StopAsync();
            this.shuttersWebService?.StopAsync();
            this.StopMoving();
        }

        private void CellsNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<CellsChangedPubSubEvent>()
                .Publish(new CellsChangedMessage(this.Cells));
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.receiveHomingUpdateToken?.Dispose();
                this.receiveHomingUpdateToken = null;

                this.elevatorPositionChangedToken?.Dispose();
                this.elevatorPositionChangedToken = null;

                this.bayChainPositionChangedToken?.Dispose();
                this.bayChainPositionChangedToken = null;

                this.positioningOperationChangedToken?.Dispose();
                this.positioningOperationChangedToken = null;

                this.shutterPositionToken?.Dispose();
                this.shutterPositionToken = null;

                this.machineModeChangedToken?.Dispose();
                this.machineModeChangedToken = null;

                this.machinePowerChangedToken?.Dispose();
                this.machinePowerChangedToken = null;

                this.moveLoadingUnitToken?.Dispose();
                this.moveLoadingUnitToken = null;

                this.fsmExceptionToken?.Dispose();
                this.fsmExceptionToken = null;
            }

            this.isDisposed = true;
        }

        private string GetActiveView()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();
            return activeView?.GetType()?.Name;
        }

        private Type GetActiveViewModelType()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();
            var model = (activeView as System.Windows.FrameworkElement)?.DataContext;
            return model?.GetType();
        }

        private async Task<MachineStatus> GetElevatorAsync(MachineStatus ms)
        {
            try
            {
                ms.EmbarkedLoadingUnit = await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync();
                ms.EmbarkedLoadingUnitId = ms.EmbarkedLoadingUnit?.Id;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            return ms;
        }

        private WarningsArea GetWarningAreaAttribute()
        {
            var viewType = this.GetActiveViewModelType();

            var attribute = viewType?.GetCustomAttributes(typeof(WarningAttribute), true)?.FirstOrDefault() as WarningAttribute;
            if (attribute is null &&
                viewType?.BaseType != null)
            {
                attribute = viewType?.BaseType?.GetCustomAttributes(typeof(WarningAttribute), true)?.FirstOrDefault() as WarningAttribute;
            }

            return attribute?.Area ?? WarningsArea.None;
        }

        private async Task InitializationBay()
        {
            this.IsMissionInError = (await this.missionsWebService.GetAllAsync()).Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);

            this.bays = await this.machineBaysWebService.GetAllAsync();

            this.cells = await this.machineCellsWebService.GetAllAsync();

            this.Bay = await this.bayManagerService.GetBayAsync();
            this.BayNumber = this.Bay.Number;

            this.HasBayExternal = this.Bay.IsExternal;

            this.HasShutter = this.Bay.Shutter.Type != ShutterType.NotSpecified;

            this.HasCarousel = this.Bay.Carousel != null;

            this.IsShutterThreeSensors = this.Bay.Shutter.Type is MAS.AutomationService.Contracts.ShutterType.ThreeSensors;

            await this.UpdateBay();
        }

        private async Task InitializationHoming()
        {
            this.IsHoming = await this.machinePowerWebService.GetIsHomingAsync();

            this.eventAggregator
                .GetEvent<HomingChangedPubSubEvent>()
                .Publish(new HomingChangedMessage(this.IsHoming));
        }

        private async Task InitializationLoadUnits()
        {
            this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
        }

        private void LoadUnitsNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<LoadUnitsChangedPubSubEvent>()
                .Publish(new LoadUnitsChangedMessage(this.Loadunits));
        }

        private void MachineStatusNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<MachineStatusChangedPubSubEvent>()
                .Publish(new MachineStatusChangedMessage(this.MachineStatus));
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.UpdateMachineStatusByElevatorPosition(e, null);
        }

        private void OnChangedEventArgs(EventArgs e)
        {
            this.WarningsManagement(this.GetActiveView());

            if (e is MachinePowerChangedEventArgs eventPower)
            {
                if (eventPower.MachinePowerState == MachinePowerState.Unpowered)
                {
                    this.StopMoving();
                }
            }

            // Dal cambiamento di qualche sensore, viene scatenato l'evento per l'aggiornamento dei dati
            if (e == EventArgs.Empty &&
                !this.MachineStatus.IsMoving)
            {
                var ms = (MachineStatus)this.MachineStatus.Clone();
                ms.FlagForNotification = !ms.FlagForNotification;
                this.MachineStatus = ms;
            }
        }

        private void OnDataChanged<TData>(NotificationMessageUI<TData> message)
            where TData : class, IMessageData
        {
            try
            {
                if (message?.Data is HomingMessageData dataHoming)
                {
                    this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");

                    Task.Run(async () =>
                            {
                                var isHoming = await this.machinePowerWebService.GetIsHomingAsync();
                                if (isHoming != this.IsHoming ||
                                isHoming && message?.Status == MessageStatus.OperationEnd ||
                                !isHoming && message?.Status == MessageStatus.OperationError)
                                {
                                    this.eventAggregator
                                    .GetEvent<HomingChangedPubSubEvent>()
                                    .Publish(new HomingChangedMessage(isHoming));
                                }
                                this.IsHoming = isHoming;
                            }).GetAwaiter().GetResult();
                }

                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                    case MessageStatus.OperationStepStart:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");
                            }

                            var ms = (MachineStatus)this.MachineStatus.Clone();

                            ms.IsStopped = false;
                            ms.MessageStatus = message.Status;
                            ms.IsError = false;
                            ms.IsMoving = true;

                            if (message?.Data is MoveLoadingUnitMessageData messageData)
                            {
                                ms.IsMovingLoadingUnit = true;

                                ms.CurrentMissionId = messageData.MissionId;

                                // TODO use messageData.MissionStep instead of message.Description
                                this.Notification = $"Movimento in corso... ({ms.CurrentMissionId} - {message.Description})";
                            }

                            if (message?.Data is PositioningMessageData dataPositioning)
                            {
                                ms.IsMovingElevator = true;

                                if (!this.MachineStatus.IsMovingLoadingUnit)
                                {
                                    this.WriteInfo(dataPositioning?.AxisMovement);
                                }

                                ms.VerticalSpeed = null;
                                if (dataPositioning.AxisMovement == Axis.Vertical)
                                {
                                    ms.VerticalTargetPosition = dataPositioning.TargetPosition;
                                    if (dataPositioning.TargetSpeed.Length > 0)
                                    {
                                        ms.VerticalSpeed = dataPositioning.TargetSpeed[0];
                                    }
                                }
                                else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                {
                                    ms.HorizontalTargetPosition = dataPositioning.TargetPosition;
                                }
                                else if (dataPositioning.AxisMovement == Axis.BayChain)
                                {
                                    ms.BayChainTargetPosition = dataPositioning.TargetPosition;
                                }
                            }

                            if (message?.Data is ShutterPositioningMessageData)
                            {
                                ms.IsMovingShutter = true;
                            }

                            this.MachineStatus = ms;
                            break;
                        }

                    case MessageStatus.OperationExecuting:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");
                            }

                            if (this.MachineStatus.IsMoving)
                            {
                                if (message?.Data is PositioningMessageData dataPositioningInfo
                                    && !this.MachineStatus.IsMovingLoadingUnit)
                                {
                                    this.WriteInfo(dataPositioningInfo?.AxisMovement);

                                    if ((dataPositioningInfo.TargetSpeed?.Length > 0 && this.MachineStatus.VerticalSpeed.HasValue && dataPositioningInfo.TargetSpeed[0] != this.MachineStatus.VerticalSpeed.Value) ||
                                        (dataPositioningInfo.AxisMovement == Axis.Vertical && this.MachineStatus.VerticalTargetPosition.HasValue && dataPositioningInfo.TargetPosition != this.MachineStatus.VerticalTargetPosition.Value) ||
                                        (dataPositioningInfo.AxisMovement == Axis.Horizontal && this.MachineStatus.HorizontalTargetPosition.HasValue && dataPositioningInfo.TargetPosition != this.MachineStatus.HorizontalTargetPosition.Value) ||
                                        (dataPositioningInfo.AxisMovement == Axis.BayChain && this.MachineStatus.BayChainTargetPosition.HasValue && dataPositioningInfo.TargetPosition != this.MachineStatus.BayChainTargetPosition.Value))
                                    {
                                        var ms = (MachineStatus)this.MachineStatus.Clone();

                                        ms.MessageStatus = message.Status;
                                        ms.VerticalSpeed = null;
                                        if (dataPositioningInfo.AxisMovement == Axis.Vertical)
                                        {
                                            ms.VerticalTargetPosition = dataPositioningInfo.TargetPosition;
                                            if (dataPositioningInfo.TargetSpeed?.Length > 0)
                                            {
                                                ms.VerticalSpeed = dataPositioningInfo.TargetSpeed[0];
                                            }
                                        }
                                        else if (dataPositioningInfo.AxisMovement == Axis.Horizontal)
                                        {
                                            ms.HorizontalTargetPosition = dataPositioningInfo.TargetPosition;
                                        }
                                        else if (dataPositioningInfo.AxisMovement == Axis.BayChain)
                                        {
                                            ms.BayChainTargetPosition = dataPositioningInfo.TargetPosition;
                                        }

                                        this.MachineStatus = ms;
                                    }
                                }

                                if (message?.Data is ShutterPositioningMessageData dataShutterPositioningInfo
                                    && !this.MachineStatus.IsMovingLoadingUnit)
                                {
                                    this.WriteInfo(null);
                                }
                            }

                            if (message?.Data is MoveLoadingUnitMessageData moveLoadingUnitMessageData)
                            {
                                this.logger.Debug($"OnMoveLoadingUnitMessageData:{moveLoadingUnitMessageData.MissionStep};");

                                // TODO use messageData.MissionStep instead of message.Description
                                this.Notification = $"Movimento in corso... ({this.MachineStatus?.CurrentMissionId} - {message.Description})";
                            }

                            break;
                        }

                    case MessageStatus.OperationUpdateData:
                        {
                            this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");

                            Task.Run(async () =>
                            {
                                this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
                                this.Cells = await this.machineCellsWebService.GetAllAsync();
                            }).GetAwaiter().GetResult();

                            var ms = (MachineStatus)this.MachineStatus.Clone();

                            ms.MessageStatus = message.Status;

                            Task.Run(async () => ms = await this.GetElevatorAsync(ms)).GetAwaiter().GetResult();

                            this.MachineStatus = ms;

                            break;
                        }

                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationStepStop:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");
                            }

                            Task.Run(async () =>
                            {
                                this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
                                this.Cells = await this.machineCellsWebService.GetAllAsync();
                                this.Bay = await this.bayManagerService.GetBayAsync();
                                if (this.MachineStatus.IsMovingLoadingUnit)
                                {
                                    this.IsMissionInError = (await this.missionsWebService.GetAllAsync()).Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);
                                }
                            }).GetAwaiter().GetResult();

                            var ms = (MachineStatus)this.MachineStatus.Clone();

                            ms.MessageStatus = message.Status;

                            if (message.Status == MessageStatus.OperationStop ||
                                message.Status == MessageStatus.OperationStepStop)
                            {
                                ms.IsStopped = true;
                            }

                            if (message?.Data is PositioningMessageData dataPositioning)
                            {
                                Task.Run(async () => ms = await this.GetElevatorAsync(ms)).GetAwaiter().GetResult();

                                ms.IsMovingElevator = false;
                                if (dataPositioning.AxisMovement == Axis.Vertical)
                                {
                                    ms.VerticalTargetPosition = null;
                                    ms.VerticalSpeed = null;
                                }
                                else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                {
                                    ms.HorizontalTargetPosition = null;
                                }
                                else if (dataPositioning.AxisMovement == Axis.BayChain)
                                {
                                    ms.BayChainTargetPosition = null;
                                }
                            }

                            ms.IsMoving = false;

                            if (message?.Data is ShutterPositioningMessageData)
                            {
                                ms.IsMovingShutter = false;
                            }

                            if (message?.Data is MoveLoadingUnitMessageData)
                            {
                                ms.IsMovingLoadingUnit = false;
                            }

                            if (!this.MachineStatus.IsMovingLoadingUnit)
                            {
                                this.ClearNotifications();
                            }

                            if (this.Bay.IsDouble || this.BayFirstPositionIsUpper)
                            {
                                if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
                                {
                                    ms.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                                    if (bayPositionUp.LoadingUnit != null)
                                    {
                                        ms.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                                    }
                                }
                            }

                            if (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)
                            {
                                if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
                                {
                                    ms.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                                    if (bayPositionDown.LoadingUnit != null)
                                    {
                                        ms.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                                    }
                                }
                            }

                            Task.Run(async () =>
                            {
                                var pos = await this.machineElevatorWebService.GetPositionAsync();

                                this.UpdateMachineStatusByElevatorPosition(
                                    new ElevatorPositionChangedEventArgs(
                                        pos.Vertical,
                                        pos.Horizontal,
                                        pos.CellId,
                                        pos.BayPositionId,
                                        pos.BayPositionUpper),
                                    ms);
                            }).GetAwaiter().GetResult();

                            this.MachineStatus = ms;

                            break;
                        }

                    case MessageStatus.OperationError:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");
                            }

                            var ms = (MachineStatus)this.MachineStatus.Clone();

                            ms.MessageStatus = message.Status;
                            ms.IsMoving = false;
                            ms.IsError = true;
                            ms.ErrorDescription = message.Description;

                            if (message?.Data is PositioningMessageData dataPositioning)
                            {
                                ms.IsMovingElevator = false;
                                if (dataPositioning.AxisMovement == Axis.Vertical)
                                {
                                    ms.VerticalTargetPosition = null;
                                    ms.VerticalSpeed = null;
                                }
                                else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                {
                                    ms.HorizontalTargetPosition = null;
                                }
                                else if (dataPositioning.AxisMovement == Axis.BayChain)
                                {
                                    ms.BayChainTargetPosition = null;
                                }
                            }

                            if (message?.Data is ShutterPositioningMessageData)
                            {
                                ms.IsMovingShutter = false;
                            }

                            if (message?.Data is MoveLoadingUnitMessageData)
                            {
                                ms.IsMovingLoadingUnit = false;
                            }

                            this.ShowNotification(message.Description, NotificationSeverity.Error);

                            this.MachineStatus = ms;
                            break;
                        }
                }

                this.WarningsManagement(this.GetActiveView());
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.UpdateMachineStatusByElevatorPosition(e, null);
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            try
            {
                await this.InitializationHoming();
                if (this.bays is null)
                {
                    await this.InitializationBay();
                }
                else
                {
                    await this.UpdateBay();
                }
                await this.InitializationLoadUnits();
            }
            catch (Exception ex)
            {
                // do nothing
            }
        }

        private void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, severity));
        }

        private void StopMoving()
        {
            var ms = (MachineStatus)this.MachineStatus.Clone();

            ms.IsMoving = false;
            ms.IsError = false;
            ms.IsMovingElevator = false;
            ms.IsMovingShutter = false;
            ms.IsMovingLoadingUnit = false;
            ms.ErrorDescription = string.Empty;

            this.MachineStatus = ms;
        }

        private void SubscribeToEvents()
        {
            this.healthStatusChangedToken = this.eventAggregator
                .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.receiveHomingUpdateToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.machineModeChangedToken = this.machineModeChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.machinePowerChangedToken = this.machinePowerChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.navigationService.SubscribeToNavigationCompleted(
               e => this.WarningsManagement(e.ViewModelName.Replace("Model", "")));

            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.fsmExceptionToken = this.fsmExceptionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                    .Subscribe(
                        this.OnDataChanged,
                        ThreadOption.UIThread,
                        false);

            this.sensorsService.OnUpdateSensors += (s, e) => this.OnChangedEventArgs(e);
        }

        private async Task UpdateBay()
        {
            this.IsMissionInError = (await this.missionsWebService.GetAllAsync()).Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);

            // Devo aggiornare i dati delle posizioni della baia
            this.Bay = await this.bayManagerService.GetBayAsync();

            var ms = (MachineStatus)this.MachineStatus.Clone();

            ms = await this.GetElevatorAsync(ms);

            ms.BayChainPosition = await this.machineCarouselWebService.GetPositionAsync();

            if (this.Bay.IsDouble || this.BayFirstPositionIsUpper)
            {
                if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
                {
                    ms.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                    if (bayPositionUp.LoadingUnit != null)
                    {
                        ms.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                    }
                }
            }

            if (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)
            {
                if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
                {
                    ms.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                    if (bayPositionDown.LoadingUnit != null)
                    {
                        ms.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                    }
                }
            }

            var pos = await this.machineElevatorWebService.GetPositionAsync();

            this.UpdateMachineStatusByElevatorPosition(
                new ElevatorPositionChangedEventArgs(
                    pos.Vertical,
                    pos.Horizontal,
                    pos.CellId,
                    pos.BayPositionId,
                    pos.BayPositionUpper),
                ms);

            this.MachineStatus = ms;
        }

        private void UpdateMachineStatusByElevatorPosition(EventArgs e, MachineStatus ms)
        {
            var update = false;
            var machineStatusNull = false;

            if (ms is null)
            {
                machineStatusNull = true;
                ms = (MachineStatus)this.MachineStatus.Clone();
            }

            if (e is ElevatorPositionChangedEventArgs dataElevatorPosition)
            {
                if (dataElevatorPosition is null)
                {
                    return;
                }

                ms.ElevatorVerticalPosition = dataElevatorPosition.VerticalPosition;
                ms.ElevatorHorizontalPosition = dataElevatorPosition.HorizontalPosition;
                ms.ElevatorPositionType = dataElevatorPosition.ElevatorPositionType;

                if (dataElevatorPosition.CellId != null)
                {
                    ms.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.CellWithNumber, dataElevatorPosition.CellId);

                    var cell = this.cells?.FirstOrDefault(l => l.Id.Equals(dataElevatorPosition.CellId));
                    if (cell != null)
                    {
                        ms.LogicalPosition = string.Concat(
                            cell?.Side.ToString(),
                            " / ",
                            cell.IsFree ? "Libera" : "Occupata",
                            cell.BlockLevel != BlockLevel.Undefined && cell.BlockLevel != BlockLevel.None ? $" / {cell.BlockLevel}" : string.Empty);
                    }
                    else
                    {
                        ms.LogicalPosition = null;
                    }
                    ms.LogicalPositionId = dataElevatorPosition.CellId;

                    ms.ElevatorPositionLoadingUnit = this.loadingUnits.FirstOrDefault(l => l.CellId.Equals(dataElevatorPosition.CellId));

                    ms.BayPositionUpper = null;
                    ms.BayPositionId = null;
                }
                else if (dataElevatorPosition.BayPositionId != null)
                {
                    var bay = this.bays.SingleOrDefault(b => b.Positions.Any(p => p.Id == dataElevatorPosition.BayPositionId));

                    ms.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.InBayWithNumber, (int)bay.Number);

                    if (dataElevatorPosition?.BayPositionUpper ?? false)
                    {
                        ms.LogicalPosition = "Posizione " + Resources.InstallationApp.PositionOnTop;
                    }
                    else
                    {
                        ms.LogicalPosition = "Posizione " + Resources.InstallationApp.PositionOnBotton;
                    }

                    ms.LogicalPositionId = (int)bay.Number;
                    ms.BayPositionUpper = (dataElevatorPosition?.BayPositionUpper ?? false);
                    ms.BayPositionId = dataElevatorPosition.BayPositionId;

                    var position = this.bay.Positions.SingleOrDefault(p => p.Id == dataElevatorPosition.BayPositionId);
                    ms.ElevatorPositionLoadingUnit = position?.LoadingUnit;
                }
                else
                {
                    ms.ElevatorPositionLoadingUnit = null;
                    ms.ElevatorLogicalPosition = null;
                    ms.LogicalPosition = null;
                    ms.LogicalPositionId = null;
                    ms.BayPositionUpper = null;
                    ms.BayPositionId = null;
                }

                update = true;
            }

            if (e is BayChainPositionChangedEventArgs dataBayChainPosition)
            {
                if (this.MachineStatus.BayChainPosition != dataBayChainPosition.Position)
                {
                    ms.BayChainPosition = dataBayChainPosition.Position;
                    update = true;
                }
            }

            if (update && machineStatusNull)
            {
                this.MachineStatus = ms;
            }
        }

        private void WarningsManagement(string view)
        {
            if (!(view is null) && !this.MachineStatus.IsMoving && !this.MachineStatus.IsMovingLoadingUnit)
            {
                switch (this.GetWarningAreaAttribute())
                {
                    case WarningsArea.MovementsView:
                    case WarningsArea.Installation:
                        if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        else if (this.sensorsService.IsHorizontalInconsistentBothLow)
                        {
                            this.ShowNotification("Manca sensore nottolino a zero o presenza cassetto.", NotificationSeverity.Error);
                        }
                        else if (this.sensorsService.IsHorizontalInconsistentBothHigh)
                        {
                            this.ShowNotification("Inconsistenza sensore nottolino a zero e presenza cassetto.", NotificationSeverity.Error);
                        }
                        else if ((this.MachineStatus.EmbarkedLoadingUnitId.GetValueOrDefault() > 0 && (this.sensorsService.IsZeroChain || !this.sensorsService.IsLoadingUnitOnElevator)) ||
                                 (this.MachineStatus.EmbarkedLoadingUnitId.GetValueOrDefault() == 0 && (!this.sensorsService.IsZeroChain || this.sensorsService.IsLoadingUnitOnElevator)))
                        {
                            this.ShowNotification("Inconsistenza stato di carico e sensori.", NotificationSeverity.Error);
                        }
                        else if (!this.IsHoming)
                        {
                            this.ShowNotification("Homing non eseguito.", NotificationSeverity.Error);
                        }
                        else if ((((this.MachineStatus.LoadingUnitPositionDownInBay != null && !this.sensorsService.IsLoadingUnitInMiddleBottomBay && (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)) ||
                                   (this.MachineStatus.LoadingUnitPositionUpInBay != null && !this.sensorsService.IsLoadingUnitInBay && (this.Bay.IsDouble || this.BayFirstPositionIsUpper))) ||
                                  ((this.MachineStatus.LoadingUnitPositionDownInBay == null && this.sensorsService.IsLoadingUnitInMiddleBottomBay && (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)) ||
                                   (this.MachineStatus.LoadingUnitPositionUpInBay == null && this.sensorsService.IsLoadingUnitInBay && (this.Bay.IsDouble || this.BayFirstPositionIsUpper)))))
                        {
                            this.ShowNotification("Inconsistenza sensori di presenza cassetto in baia.", NotificationSeverity.Error);
                        }
                        else if ((view.Equals("VerticalResolutionCalibrationView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("VerticalOffsetCalibrationView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("BayCheckView", StringComparison.InvariantCultureIgnoreCase)) &&
                                 this.sensorsService.IsLoadingUnitOnElevator)
                        {
                            this.ShowNotification("Presenza cassetto sull'elevatore.", NotificationSeverity.Warning);
                        }
                        // tranne per la macchina con la baia esterna l'elevatore non si può muovere se c'è la serranda aperta
                        else if (!this.bay.IsExternal &&
                                 !this.sensorsService.ShutterSensors.Closed)
                        {
                            this.ShowNotification("Serranda non completamente chiusa.", NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification("Missioni in errore...", NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.Picking:
                        if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachineMode != MachineMode.Automatic)
                        {
                            this.ShowNotification("Manca automatico.", NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification("Missioni in errore...", NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.Maintenance:
                        if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        break;

                    case WarningsArea.Information:
                    case WarningsArea.Menu:
                        if (this.IsMissionInError)
                        {
                            this.ShowNotification("Missioni in errore...", NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void WriteInfo(Axis? axisMovement)
        {
            var view = this.GetWarningAreaAttribute();
            if (view == WarningsArea.None ||
                view == WarningsArea.Login ||
                view == WarningsArea.Menu ||
                view == WarningsArea.Maintenance ||
                view == WarningsArea.Information ||
                view == WarningsArea.Picking)
            {
                return;
            }

            if (this.MachineStatus.IsMovingShutter)
            {
                if (this.machineModeService.MachineMode == MachineMode.Test)
                {
                    this.Notification = "Test in corso...";
                }
                else
                {
                    this.Notification = "Movimento serranda in corso...";
                }
            }
            else
            {
                if (this.machineModeService.MachineMode == MachineMode.Manual)
                {
                    if (axisMovement.HasValue && axisMovement == Axis.Vertical)
                    {
                        this.Notification = "Movimento verticale in corso...";
                    }
                    else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
                    {
                        this.Notification = "Movimento orizzontale in corso...";
                    }
                    else if (axisMovement.HasValue && axisMovement == Axis.BayChain)
                    {
                        this.Notification = "Movimento catena baia in corso...";
                    }
                }
                else if (this.machineModeService.MachineMode == MachineMode.Test)
                {
                    this.Notification = "Test in corso...";
                }
                else
                {
                    this.Notification = "Movimento in corso...";
                }
            }
        }

        #endregion
    }
}
