using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;
using ShutterType = Ferretto.VW.MAS.AutomationService.Contracts.ShutterType;

namespace Ferretto.VW.App.Services
{
    public class MachineService : BindableBase, IMachineService, IDisposable
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

        private SubscriptionToken fsmExceptionToken;

        private bool hasBayExternal;

        private bool hasCarousel;

        private bool hasShutter;

        private bool isDisposed;

        private bool isHoming;

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
            IBayManager bayManagerService)
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
        }

        #endregion

        #region Properties

        public Bay Bay
        {
            get => this.bay;
            private set => this.SetProperty(ref this.bay, value);
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

        protected NLog.Logger Logger => this.logger;

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

        private bool IsHealthy => this.healthProbeService?.HealthStatus == HealthStatus.Healthy;

        #endregion

        #region Methods

        public void ClearNotifications()
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(true));
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public async Task OnUpdateServiceAsync()
        {
            await this.InitializationHoming();
            await this.InitializationBay();
            await this.InitializationLoadUnits();
        }

        public void ServiceStart()
        {
            this.machineStatus = new MachineStatus();
            this.loadingUnits = new List<LoadingUnit>();

            this.OnUpdateServiceAsync().ConfigureAwait(false);

            this.SubscribeToEvents();
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.Logger.Error(exception);

            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
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
                ms.EmbarkedLoadingUnitId = ms.EmbarkedLoadingUnit?.Id.ToString();
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
            if (viewType is null)
            {
                string s = "";
            }

            WarningsArea area = WarningsArea.None;
            WarningAttribute attribute = viewType?.GetCustomAttributes(typeof(WarningAttribute), true)?.FirstOrDefault() as WarningAttribute;

            if (attribute is null &&
                viewType.BaseType != null)
            {
                attribute = viewType?.BaseType?.GetCustomAttributes(typeof(WarningAttribute), true)?.FirstOrDefault() as WarningAttribute;
            }

            return attribute?.Area ?? WarningsArea.None;
        }

        private async Task InitializationBay()
        {
            this.bays = await this.machineBaysWebService.GetAllAsync();

            this.cells = await this.machineCellsWebService.GetAllAsync();

            this.Bay = await this.bayManagerService.GetBayAsync();
            this.BayNumber = this.Bay.Number;

            this.HasBayExternal = this.Bay.IsExternal;

            this.HasShutter = this.Bay.Shutter.Type != ShutterType.NotSpecified;

            this.HasCarousel = this.Bay.Carousel != null;

            this.IsShutterThreeSensors = this.Bay.Shutter.Type is MAS.AutomationService.Contracts.ShutterType.ThreeSensors;

            var ms = (MachineStatus)this.MachineStatus.Clone();

            ms = await this.GetElevatorAsync(ms);

            ms.BayChainPosition = await this.machineCarouselWebService.GetPositionAsync();

            if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
            {
                ms.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                if (bayPositionDown.LoadingUnit != null)
                {
                    ms.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                }
            }

            if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
            {
                ms.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                if (bayPositionUp.LoadingUnit != null)
                {
                    ms.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                }
            }

            this.MachineStatus = ms;

            var pos = await this.machineElevatorWebService.GetPositionAsync();

            this.UpdateMachineStatus(
                new ElevatorPositionChangedEventArgs(
                    pos.Vertical,
                    pos.Horizontal,
                    pos.CellId,
                    pos.BayPositionId,
                    pos.BayPositionUpper));
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
            this.UpdateMachineStatus(e);
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
        }

        private void OnDataChanged<TData>(NotificationMessageUI<TData> message)
            where TData : class, IMessageData
        {
            if (message?.Data is HomingMessageData dataHoming)
            {
                this.logger.Debug($"OnDataChanged:{typeof(TData).Name}; {message.Status};");

                this.machinePowerWebService.GetIsHomingAsync()
                    .ContinueWith((m) =>
                    {
                        if (!m.IsFaulted)
                        {
                            bool isHoming = m.Result;
                            if (isHoming != this.IsHoming ||
                                isHoming && message?.Status == MessageStatus.OperationEnd ||
                                !isHoming && message?.Status == MessageStatus.OperationError)
                            {
                                this.eventAggregator
                                    .GetEvent<HomingChangedPubSubEvent>()
                                    .Publish(new HomingChangedMessage(isHoming));
                            }

                            this.IsHoming = isHoming;
                        }
                    });
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

                        ms.IsError = false;
                        ms.IsMoving = true;

                        if (message?.Data is MoveLoadingUnitMessageData messageData)
                        {
                            ms.IsMovingLoadingUnit = true;

                            ms.CurrentMissionId = messageData.MissionId;

                            this.Notification = "Movimento in corso...";
                        }

                        if (message?.Data is PositioningMessageData dataPositioning)
                        {
                            ms.IsMovingElevator = true;

                            if (!this.MachineStatus.IsMovingLoadingUnit)
                            {
                                this.WriteInfo(dataPositioning?.AxisMovement);
                            }

                            if (dataPositioning.AxisMovement == Axis.Vertical)
                            {
                                ms.VerticalTargetPosition = dataPositioning.TargetPosition;
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
                        if (this.MachineStatus.IsMoving)
                        {
                            if (message?.Data is PositioningMessageData dataPositioningInfo
                                && !this.MachineStatus.IsMovingLoadingUnit)
                            {
                                this.WriteInfo(dataPositioningInfo?.AxisMovement);
                            }
                        }

                        if (this.MachineStatus.IsMovingLoadingUnit)
                        {
                            this.Notification = "Movimento in corso...";
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
                        }).Wait();

                        var ms = (MachineStatus)this.MachineStatus.Clone();

                        Task.Run(async () => ms = await this.GetElevatorAsync(ms)).Wait();

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
                        }).Wait();

                        var ms = (MachineStatus)this.MachineStatus.Clone();

                        if (message?.Data is PositioningMessageData dataPositioning)
                        {
                            Task.Run(async () => ms = await this.GetElevatorAsync(ms)).Wait();

                            ms.IsMovingElevator = false;
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

                        ms.VerticalTargetPosition = null;
                        ms.HorizontalTargetPosition = null;
                        ms.BayChainTargetPosition = null;

                        if (!this.MachineStatus.IsMovingLoadingUnit)
                        {
                            this.ClearNotifications();
                        }

                        if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
                        {
                            ms.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                            if (bayPositionDown.LoadingUnit != null)
                            {
                                ms.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                            }
                        }

                        if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
                        {
                            ms.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                            if (bayPositionUp.LoadingUnit != null)
                            {
                                ms.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                            }
                        }

                        this.MachineStatus = ms;

                        Task.Run(async () =>
                        {
                            var pos = await this.machineElevatorWebService.GetPositionAsync();

                            this.UpdateMachineStatus(
                                new ElevatorPositionChangedEventArgs(
                                    pos.Vertical,
                                    pos.Horizontal,
                                    pos.CellId,
                                    pos.BayPositionId,
                                    pos.BayPositionUpper));
                        }).Wait();

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

                        ms.IsMoving = false;
                        ms.IsError = true;
                        ms.ErrorDescription = message.Description;

                        if (message?.Data is PositioningMessageData dataPositioning)
                        {
                            ms.IsMovingElevator = false;
                        }

                        if (message?.Data is ShutterPositioningMessageData)
                        {
                            ms.IsMovingShutter = false;
                        }

                        if (message?.Data is MoveLoadingUnitMessageData)
                        {
                            ms.IsMovingLoadingUnit = false;
                        }

                        ms.VerticalTargetPosition = null;
                        ms.HorizontalTargetPosition = null;
                        ms.BayChainTargetPosition = null;

                        this.ShowNotification(message.Description, NotificationSeverity.Error);

                        this.MachineStatus = ms;
                        break;
                    }
            }

            this.WarningsManagement(this.GetActiveView());
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.UpdateMachineStatus(e);
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
        }

        private void UpdateMachineStatus(EventArgs e)
        {
            bool update = false;

            var ms = (MachineStatus)this.MachineStatus.Clone();

            if (e is ElevatorPositionChangedEventArgs dataElevatorPosition)
            {
                if (dataElevatorPosition is null)
                {
                    return;
                }

                ms.ElevatorVerticalPosition = dataElevatorPosition.VerticalPosition;
                ms.ElevatorHorizontalPosition = dataElevatorPosition.HorizontalPosition;

                if (dataElevatorPosition.CellId != null)
                {
                    ms.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.CellWithNumber, dataElevatorPosition.CellId);

                    var cell = this.cells?.FirstOrDefault(l => l.Id.Equals(dataElevatorPosition.CellId));
                    ms.LogicalPosition = cell?.Status.ToString() + " / " + cell?.Side.ToString();
                    ms.LogicalPositionId = dataElevatorPosition.CellId.ToString();

                    ms.ElevatorPositionLoadingUnit = this.loadingUnits.FirstOrDefault(l => l.CellId.Equals(dataElevatorPosition.CellId));
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

                    ms.LogicalPositionId = ((int)bay.Number).ToString();
                }
                else
                {
                    ms.ElevatorPositionLoadingUnit = null;
                    ms.ElevatorLogicalPosition = null;
                    ms.LogicalPosition = null;
                    ms.LogicalPositionId = null;
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

            if (update)
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
                    case WarningsArea.Installation:
                        if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        else if (!this.IsHoming)
                        {
                            this.ShowNotification("Homing non eseguito.", NotificationSeverity.Error);
                        }
                        else if (this.MachineStatus.BayChainPosition is null)
                        {
                            this.ShowNotification("Posizione catena sconosciuta.", NotificationSeverity.Error);
                        }
                        else if (string.IsNullOrEmpty(this.MachineStatus.ElevatorLogicalPosition))
                        {
                            this.ShowNotification("Posizione elevatore sconosciuta.", NotificationSeverity.Low);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.Maintenance:
                        if (!this.IsHoming)
                        {
                            this.ShowNotification("Homing non eseguito.", NotificationSeverity.Error);
                        }
                        break;

                    case WarningsArea.Picking:
                        if (this.machineModeService.MachineMode != MachineMode.Automatic)
                        {
                            this.ShowNotification("Manca automatico.", NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification("Manca marcia.", NotificationSeverity.Warning);
                        }
                        break;

                    case WarningsArea.Information:
                    case WarningsArea.Menu:
                        this.ClearNotifications();
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

        #endregion
    }
}
