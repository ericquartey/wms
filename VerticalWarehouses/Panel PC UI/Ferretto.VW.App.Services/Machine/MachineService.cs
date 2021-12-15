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
    public sealed class MachineService : BindableBase, IMachineService, IDisposable
    {
        #region Fields

        private const int MaximumFragmentation = 50;

        private readonly IBayManager bayManagerService;

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly Dictionary<Axis, bool> isHomingStarted;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly INavigationService navigationService;

        private readonly IRegionManager regionManager;

        private readonly ISensorsService sensorsService;

        private readonly ISessionService sessionService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private string activeView;

        private Bay bay;

        private SubscriptionToken bayChainPositionChangedToken;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private IEnumerable<Bay> bays;

        private IEnumerable<Cell> cells;

        private SubscriptionToken combinedMovementsChangedToken;

        private SubscriptionToken elevatorPositionChangedToken;

        private bool executedInitialization;

        private SubscriptionToken fsmExceptionToken;

        private bool hasBayExternal;

        private bool hasBayWithInverter;

        private bool hasCarousel;

        private bool hasShutter;

        private SubscriptionToken healthStatusChangedToken;

        private bool isAnyExpired;

        private bool isAnyExpiring;

        private bool isAxisTuningCompleted;

        private Dictionary<MAS.AutomationService.Contracts.BayNumber, bool> isBayHoming;

        private bool isDisposed;

        private bool isHoming;

        private bool isMissionInError;

        private bool isMissionInErrorByLoadUnitOperations;

        private bool isMissionWms;

        private bool isShutterThreeSensors;

        private bool isTuningCompleted;

        private IEnumerable<LoadingUnit> loadingUnits;

        private double loadUnitMaxHeight;

        private SubscriptionToken machineModeChangedToken;

        private SubscriptionToken machinePowerChangedToken;

        private Models.MachineStatus machineStatus;

        private SubscriptionToken moveLoadingUnitToken;

        private string notification;

        private IEnumerable<Cell> originalCells;

        private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken receiveHomingUpdateToken;

        private SubscriptionToken repetitiveHorizontalMovementsToken;

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
            IMachineMissionsWebService missionsWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineSetupStatusWebService machineSetupStatusWebService,
            IMachineServicingWebService machineServicingWebService,
            IMachineModeWebService machineModeWebService,
            ISessionService sessionService)
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
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineSetupStatusWebService = machineSetupStatusWebService ?? throw new ArgumentNullException(nameof(machineSetupStatusWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));

            this.MachineStatus = new Models.MachineStatus();

            this.isBayHoming = new Dictionary<MAS.AutomationService.Contracts.BayNumber, bool>();
            this.isHomingStarted = new Dictionary<Axis, bool>();
            this.isHomingStarted.Add(Axis.Horizontal, false);
            this.isHomingStarted.Add(Axis.Vertical, false);
            this.isHomingStarted.Add(Axis.HorizontalAndVertical, false);
            this.isHomingStarted.Add(Axis.BayChain, false);
        }

        #endregion

        #region Properties

        public string ActiveView
        {
            get => this.activeView;
            set => this.SetProperty(ref this.activeView, value);
        }

        public Bay Bay
        {
            get => this.bay;
            private set => this.SetProperty(ref this.bay, value);
        }

        public bool BayFirstPositionIsUpper => this.bay?.Positions?.FirstOrDefault()?.IsUpper ?? false;

        public MAS.AutomationService.Contracts.BayNumber BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value);
        }

        public IEnumerable<Bay> Bays
        {
            get => this.bays;
            private set => this.SetProperty(ref this.bays, value);
        }

        public IEnumerable<Cell> Cells
        {
            get => this.cells;
            set => this.SetProperty(ref this.cells, value, this.CellsNotificationProperty);
        }

        public List<CellPlus> CellsPlus { get; set; }

        public double FragmentTotalPercent { get; private set; }

        public double FreeSpaceBack { get; private set; }

        public double FreeSpaceFront { get; private set; }

        public bool HasBayExternal
        {
            get => this.hasBayExternal;
            set => this.SetProperty(ref this.hasBayExternal, value);
        }

        public bool HasBayWithInverter
        {
            get => this.hasBayWithInverter;
            set => this.SetProperty(ref this.hasBayWithInverter, value);
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

        public bool IsAnyExpired
        {
            get => this.isAnyExpired;
            set => this.SetProperty(ref this.isAnyExpired, value);
        }

        public bool IsAnyExpiring
        {
            get => this.isAnyExpiring;
            set => this.SetProperty(ref this.isAnyExpiring, value);
        }

        public bool IsAxisTuningCompleted
        {
            get => this.isAxisTuningCompleted;
            private set => this.SetProperty(ref this.isAxisTuningCompleted, value);
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

        public bool IsMissionInErrorByLoadUnitOperations
        {
            get => this.isMissionInErrorByLoadUnitOperations;
            private set => this.SetProperty(ref this.isMissionInErrorByLoadUnitOperations, value);
        }

        public bool IsMissionWms
        {
            get => this.isMissionWms;
            private set => this.SetProperty(ref this.isMissionWms, value);
        }

        public bool IsShutterThreeSensors
        {
            get => this.isShutterThreeSensors;
            set => this.SetProperty(ref this.isShutterThreeSensors, value);
        }

        public bool IsTuningCompleted
        {
            get => this.isTuningCompleted;
            private set => this.SetProperty(ref this.isTuningCompleted, value);
        }

        public bool IsWmsEnabled => this.machineModeService.IsWmsEnabled;

        public IEnumerable<LoadingUnit> Loadunits
        {
            get => this.loadingUnits;
            set => this.SetProperty(ref this.loadingUnits, value, this.LoadUnitsNotificationProperty);
        }

        public MachineMode MachineMode => this.machineModeService.MachineMode;

        public MachinePowerState MachinePower => this.machineModeService.MachinePower;

        public Models.MachineStatus MachineStatus { get; }

        public double MaxSolidSpace { get; private set; }

        public IEnumerable<Cell> OriginalCells => this.originalCells;

        internal bool LUPresentInBay1 => (this.Bay.IsExternal || this.Bay.Carousel == null) && this.Bay.IsDouble ? (this.sensorsService.Sensors.LUPresentInBay1 || this.sensorsService.Sensors.LUPresentMiddleBottomBay1) : this.sensorsService.Sensors.LUPresentInBay1;

        internal bool LUPresentInBay2 => (this.Bay.IsExternal || this.Bay.Carousel == null) && this.Bay.IsDouble ? (this.sensorsService.Sensors.LUPresentInBay2 || this.sensorsService.Sensors.LUPresentMiddleBottomBay2) : this.sensorsService.Sensors.LUPresentInBay2;

        internal bool LUPresentInBay3 => (this.Bay.IsExternal || this.Bay.Carousel == null) && this.Bay.IsDouble ? (this.sensorsService.Sensors.LUPresentInBay3 || this.sensorsService.Sensors.LUPresentMiddleBottomBay3) : this.sensorsService.Sensors.LUPresentInBay3;

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

        public async Task GetCells()
        {
            this.Cells = await this.machineCellsWebService.GetAllAsync();
            if (this.originalCells is null)
            {
                this.originalCells = await this.machineCellsWebService.GetAllAsync();
            }
            this.CellsPlus = this.Cells.Select(c => new CellPlus(c, this.Loadunits.FirstOrDefault(l => l.CellId == c.Id))).ToList();
            this.FreeSpaceBack = this.cells.Count(s => s.IsFree && (s.BlockLevel == BlockLevel.None || s.BlockLevel == BlockLevel.SpaceOnly) && s.Side == WarehouseSide.Back) * 25;
            this.FreeSpaceFront = this.cells.Count(s => s.IsFree && (s.BlockLevel == BlockLevel.None || s.BlockLevel == BlockLevel.SpaceOnly) && s.Side == WarehouseSide.Front) * 25;
            var cellStat = await this.machineCellsWebService.GetStatisticsAsync();
            this.MaxSolidSpace = cellStat.MaxSolidSpace.Select(s => s.Value).Max();
            this.UpdateLoadUnitsId();
            if (this.loadUnitMaxHeight == 0)
            {
                this.loadUnitMaxHeight = await this.machineLoadingUnitsWebService.GetLoadUnitMaxHeightAsync();
            }
        }

        public async Task GetLoadUnits()
        {
            this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
        }

        public async Task GetTuningStatus()
        {
            try
            {
                var idService = await this.machineIdentityWebService.GetAsync();
                if (idService != null)
                {
                    this.sessionService.MachineIdentity = idService;
                    this.IsTuningCompleted = idService.InstallationDate.HasValue;
                }

                var setupStatus = await this.machineSetupStatusWebService.GetAsync();
                this.IsAxisTuningCompleted = setupStatus.VerticalOriginCalibration.IsCompleted &&
                    setupStatus.VerticalResolutionCalibration.IsCompleted &&
                    setupStatus.VerticalOffsetCalibration.IsCompleted;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public bool IsSensorMissing()
        {
            switch (this.Bay.Number)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne: return !this.LUPresentInBay1 && this.Bay.CurrentMission != null;
                case MAS.AutomationService.Contracts.BayNumber.BayTwo: return !this.LUPresentInBay2 && this.Bay.CurrentMission != null;
                case MAS.AutomationService.Contracts.BayNumber.BayThree: return !this.LUPresentInBay3 && this.Bay.CurrentMission != null;
                default: return false;
            }
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
                    await this.GetLoadUnits();
                    await this.GetTuningStatus();
                    await this.GetCells();

                    this.IsAnyExpiring = await this.machineServicingWebService.IsInstructionExpiringAsync();
                    this.IsAnyExpired = await this.machineServicingWebService.IsInstructionExpiredAsync();
                }
                catch
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
                await this.GetLoadUnits();
                await this.GetCells();
                await this.machineModeService.OnUpdateServiceAsync();
                await this.GetTuningStatus();

                this.IsAnyExpiring = await this.machineServicingWebService.IsInstructionExpiringAsync();
                this.IsAnyExpired = await this.machineServicingWebService.IsInstructionExpiredAsync();
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

        public async Task ShutdownAsync()
        {
            if (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                await this.machineModeWebService.SetShutdownAsync();
            }
            else
            {
                this.logger.Warn("Shutdown pc");
                var psi = new ProcessStartInfo("shutdown", "/s /t 5");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);

                this.logger.Warn("Close application");
                System.Windows.Application.Current.Shutdown();
            }
        }

        public async Task StartAsync()
        {
            this.machineStatus = new Models.MachineStatus();
            this.loadingUnits = new List<LoadingUnit>();

            this.SubscribeToEvents();

            try
            {
                await this.OnInitializationServiceAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
            }
            catch (Exception)
            {
            }
        }

        public async Task StopMovingByAllAsync()
        {
            if (this.machineStatus.CurrentMissionId != null)
            {
                await this.machineLoadingUnitsWebService?.StopAsync(this.machineStatus.CurrentMissionId, this.BayNumber);
            }

            await Task.WhenAll(
                this.machineElevatorWebService?.StopAsync(),
                this.machineCarouselWebService?.StopAsync(),
                this.shuttersWebService?.StopAsync());

            this.StopMoving();
        }

        public async Task UpdateLoadUnitInBayAsync()
        {
            this.Bay = await this.bayManagerService.GetBayAsync();
            if (this.Bay.IsDouble || this.BayFirstPositionIsUpper || this.Bay.Carousel != null)
            {
                if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
                {
                    this.MachineStatus.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                    if (bayPositionUp.LoadingUnit != null)
                    {
                        this.MachineStatus.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                        this.logger.Debug($"UpdateLoadUnitInBay({this.BayNumber}): position ({bayPositionUp.Location}), LU({bayPositionUp.LoadingUnit?.Id})");
                    }
                }
            }

            if (this.Bay.IsDouble || (!this.BayFirstPositionIsUpper))
            {
                if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
                {
                    this.MachineStatus.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                    if (bayPositionDown.LoadingUnit != null)
                    {
                        this.MachineStatus.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                        this.logger.Debug($"UpdateLoadUnitInBay({this.BayNumber}): position ({bayPositionDown.Location}), LU({bayPositionDown.LoadingUnit?.Id})");
                    }
                }
            }
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

                this.combinedMovementsChangedToken?.Dispose();
                this.combinedMovementsChangedToken = null;

                this.healthStatusChangedToken?.Dispose();
                this.healthStatusChangedToken = null;

                this.shutterPositionToken?.Dispose();
                this.shutterPositionToken = null;

                this.repetitiveHorizontalMovementsToken?.Dispose();
                this.repetitiveHorizontalMovementsToken = null;

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

        private async Task<LoadingUnit> GetLodingUnitOnBoardAsync()
        {
            try
            {
                return await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            return null;
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
            try
            {
                var missions = (await this.missionsWebService.GetAllAsync());
                this.IsMissionInError = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);

                this.IsMissionInErrorByLoadUnitOperations = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined && a.MissionType == MAS.AutomationService.Contracts.MissionType.LoadUnitOperation);

                this.IsMissionWms = missions.Any(a => a.MissionType == MAS.AutomationService.Contracts.MissionType.WMS);

                this.MachineStatus.IsMovingLoadingUnit = missions.Any(m => m.Status == MAS.AutomationService.Contracts.MissionStatus.Executing);

                this.bays = await this.machineBaysWebService.GetAllAsync();

                this.Bay = await this.bayManagerService.GetBayAsync();

                this.BayNumber = this.Bay.Number;

                this.HasBayExternal = this.Bay.IsExternal;

                this.HasShutter = (this.Bay.Shutter != null) ? this.Bay.Shutter.Type != ShutterType.NotSpecified : false;

                this.HasCarousel = this.Bay.Carousel != null;

                this.HasBayWithInverter = this.Bay.Inverter != null;

                this.IsShutterThreeSensors = (this.Bay.Shutter != null) ? this.Bay.Shutter.Type is MAS.AutomationService.Contracts.ShutterType.ThreeSensors : false;

                await this.UpdateBay();
            }
            catch (Exception exc)
            {
                this.logger.Debug($"Exception description: {exc.Message}");
            }
        }

        private async Task InitializationHoming()
        {
            this.isBayHoming = await this.machinePowerWebService.GetIsHomingAsync();

            if (this.isBayHoming != null && this.isBayHoming.ContainsKey(MAS.AutomationService.Contracts.BayNumber.ElevatorBay))
            {
                this.IsHoming = this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay];
            }

            this.eventAggregator
                .GetEvent<HomingChangedPubSubEvent>()
                .Publish(new HomingChangedMessage(this.IsHoming));
        }

        private void LoadUnitsNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<LoadUnitsChangedPubSubEvent>()
                .Publish(new LoadUnitsChangedMessage(this.Loadunits));
        }

        private void NotifyMachineStatusChanged()
        {
            this.eventAggregator
                .GetEvent<MachineStatusChangedPubSubEvent>()
                .Publish(new MachineStatusChangedMessage(this.MachineStatus));
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.UpdateMachineStatusByElevatorPosition(e);
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
                lock (this.MachineStatus)
                {
                    this.MachineStatus.FlagForNotification = !this.MachineStatus.FlagForNotification;
                }

                this.NotifyMachineStatusChanged();
            }
        }

        private async Task OnDataChangedAsync<TData>(NotificationMessageUI<TData> message)
            where TData : class, IMessageData
        {
            try
            {
                if (message?.Data is HomingMessageData dataHoming)
                {
                    this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");

                    this.isBayHoming = await this.machinePowerWebService.GetIsHomingAsync();
                    if (this.isBayHoming != null && this.isBayHoming.ContainsKey(MAS.AutomationService.Contracts.BayNumber.ElevatorBay)
                        && (this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay] != this.IsHoming
                            || (this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay] && message?.Status == MessageStatus.OperationEnd)
                            || (!this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay] && message?.Status == MessageStatus.OperationError)
                            )
                        )
                    {
                        this.eventAggregator
                        .GetEvent<HomingChangedPubSubEvent>()
                        .Publish(new HomingChangedMessage(this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay]));
                    }
                    this.IsHoming = this.isBayHoming[MAS.AutomationService.Contracts.BayNumber.ElevatorBay];
                    switch (message?.Status)
                    {
                        case MessageStatus.OperationStart:
                            if (this.isHomingStarted.ContainsKey(dataHoming.AxisToCalibrate))
                            {
                                this.isHomingStarted[dataHoming.AxisToCalibrate] = true;
                            }
                            break;

                        case MessageStatus.OperationEnd:
                        case MessageStatus.OperationError:
                            if (this.isHomingStarted.ContainsKey(dataHoming.AxisToCalibrate))
                            {
                                this.isHomingStarted[dataHoming.AxisToCalibrate] = false;
                            }
                            break;
                    }
                }

                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                    case MessageStatus.OperationStepStart:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");
                            }

                            lock (this.MachineStatus)
                            {
                                this.MachineStatus.IsStopped = false;
                                this.MachineStatus.MessageStatus = message.Status;
                                this.MachineStatus.IsError = false;
                                this.MachineStatus.IsMoving = true;

                                if (message?.Data is MoveLoadingUnitMessageData messageData)
                                {
                                    this.MachineStatus.IsMovingLoadingUnit = true;

                                    this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; IsMovingLoadingUnit({this.MachineStatus.IsMovingLoadingUnit});");

                                    this.MachineStatus.CurrentMission = messageData;
                                    this.MachineStatus.CurrentMissionId = messageData.MissionId;
                                    this.MachineStatus.CurrentMissionDescription = message.Description;

                                    // TODO use messageData.MissionStep instead of message.Description
                                    //var msg = string.Format(ServiceMachine.MovementInProgress, $"(Missione: {this.MachineStatus.CurrentMissionId}, " +
                                    //    $"Cassetto: {messageData.LoadUnitId}, " +
                                    //    $"Stato: {message.Description}, " +
                                    //    $"Movimento: da {messageData.Source} {messageData.SourceCellId}, " +
                                    //    $"a {messageData.Destination}" +
                                    //    $"{(messageData.DestinationCellId is null ? string.Empty : " ")}{messageData.DestinationCellId}" +
                                    //    $")");
                                    var msg = string.Format(Resources.Localized.Get("ServiceMachine.MovementInProgressExtended"),
                                        this.MachineStatus.CurrentMissionId,
                                        messageData.LoadUnitId,
                                        message.Description,
                                        messageData.Source,
                                        messageData.SourceCellId,
                                        messageData.Destination,
                                        messageData.DestinationCellId is null ? string.Empty : " " + messageData.DestinationCellId
                                        );

                                    if (this.sessionService.UserAccessLevel != MAS.AutomationService.Contracts.UserAccessLevel.Operator)
                                    {
                                        this.Notification = msg;
                                    }
                                    else
                                    {
                                        this.logger.Debug(msg);
                                    }
                                    //this.Notification = string.Format(ServiceMachine.MovementInProgress, $"(Missione: {this.MachineStatus.CurrentMissionId}, " +
                                    //    $"Cassetto: {messageData.LoadUnitId}, " +
                                    //    $"Stato: {message.Description}, " +
                                    //    $"Movimento: da {messageData.Source} {messageData.SourceCellId} " +
                                    //    $"a {messageData.Destination} {messageData.DestinationCellId})");
                                }

                                if (message?.Data is PositioningMessageData dataPositioning)
                                {
                                    this.MachineStatus.IsMovingElevator = true;

                                    if (!this.MachineStatus.IsMovingLoadingUnit)
                                    {
                                        this.WriteInfo(dataPositioning?.AxisMovement);
                                    }

                                    this.MachineStatus.VerticalSpeed = null;
                                    if (dataPositioning.AxisMovement == Axis.Vertical)
                                    {
                                        this.MachineStatus.VerticalTargetPosition = dataPositioning.TargetPosition;
                                        if (dataPositioning.TargetSpeed.Length > 0)
                                        {
                                            this.MachineStatus.VerticalSpeed = dataPositioning.TargetSpeed[0];
                                        }
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                    {
                                        this.MachineStatus.HorizontalTargetPosition = dataPositioning.TargetPosition;
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.BayChain)
                                    {
                                        this.MachineStatus.BayChainTargetPosition = dataPositioning.TargetPosition;
                                    }
                                }

                                if (message?.Data is ShutterPositioningMessageData)
                                {
                                    this.MachineStatus.IsMovingShutter = true;
                                }
                            }

                            this.NotifyMachineStatusChanged();
                            break;
                        }

                    case MessageStatus.OperationExecuting:
                    case MessageStatus.OperationWaitResume:
                        {
                            //if (message?.Data is PositioningMessageData dataLog)
                            //{
                            //    this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            //}
                            //else
                            //{
                            //    this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");
                            //}

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
                                        lock (this.MachineStatus)
                                        {
                                            this.MachineStatus.MessageStatus = message.Status;
                                            this.MachineStatus.VerticalSpeed = null;
                                            if (dataPositioningInfo.AxisMovement == Axis.Vertical)
                                            {
                                                this.MachineStatus.VerticalTargetPosition = dataPositioningInfo.TargetPosition;
                                                if (dataPositioningInfo.TargetSpeed?.Length > 0)
                                                {
                                                    this.MachineStatus.VerticalSpeed = dataPositioningInfo.TargetSpeed[0];
                                                }
                                            }
                                            else if (dataPositioningInfo.AxisMovement == Axis.Horizontal)
                                            {
                                                this.MachineStatus.HorizontalTargetPosition = dataPositioningInfo.TargetPosition;
                                            }
                                            else if (dataPositioningInfo.AxisMovement == Axis.BayChain)
                                            {
                                                this.MachineStatus.BayChainTargetPosition = dataPositioningInfo.TargetPosition;
                                            }
                                        }

                                        this.NotifyMachineStatusChanged();
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

                                lock (this.MachineStatus)
                                {
                                    this.MachineStatus.CurrentMission = moveLoadingUnitMessageData;
                                    this.MachineStatus.CurrentMissionId = moveLoadingUnitMessageData.MissionId;
                                    this.MachineStatus.CurrentMissionDescription = message.Description;
                                    this.MachineStatus.IsMovingLoadingUnit = moveLoadingUnitMessageData.MissionStep != CommonUtils.Messages.Enumerations.MissionStep.WaitPick;
                                }

                                this.NotifyMachineStatusChanged();

                                // TODO use messageData.MissionStep instead of message.Description
                                //var msg = string.Format(ServiceMachine.MovementInProgress, $"(Missione: {this.MachineStatus?.CurrentMissionId}, " +
                                //    $"Cassetto: {moveLoadingUnitMessageData.LoadUnitId}, " +
                                //    $"Stato: {message.Description}, " +
                                //    $"Movimento: da {moveLoadingUnitMessageData.Source} {moveLoadingUnitMessageData.SourceCellId}, " +
                                //    $"a {moveLoadingUnitMessageData.Destination}" +
                                //    $"{(moveLoadingUnitMessageData.DestinationCellId is null ? string.Empty : " ")}{moveLoadingUnitMessageData.DestinationCellId}" +
                                //    $")");

                                var msg = string.Format(Resources.Localized.Get("ServiceMachine.MovementInProgressExtended"),
                                    this.MachineStatus?.CurrentMissionId,
                                    moveLoadingUnitMessageData.LoadUnitId,
                                    message.Description,
                                    moveLoadingUnitMessageData.Source,
                                    moveLoadingUnitMessageData.SourceCellId,
                                    moveLoadingUnitMessageData.Destination,
                                    moveLoadingUnitMessageData.DestinationCellId is null ? string.Empty : " " + moveLoadingUnitMessageData.DestinationCellId
                                    );

                                if (this.sessionService.UserAccessLevel != MAS.AutomationService.Contracts.UserAccessLevel.Operator)
                                {
                                    this.Notification = msg;
                                }
                                else
                                {
                                    this.logger.Debug(msg);
                                    //if (message.Status == MessageStatus.OperationWaitResume)
                                    //{
                                    //    this.ClearNotifications();
                                    //}
                                }
                                //this.Notification = string.Format(ServiceMachine.MovementInProgress, $"(Missione: {this.MachineStatus?.CurrentMissionId}, " +
                                //    $"Cassetto: {moveLoadingUnitMessageData.LoadUnitId}, " +
                                //    $"Stato: {message.Description}, " +
                                //    $"Movimento: da {moveLoadingUnitMessageData.Source} {moveLoadingUnitMessageData.SourceCellId} " +
                                //    $"a {moveLoadingUnitMessageData.Destination} {moveLoadingUnitMessageData.DestinationCellId})");
                            }

                            break;
                        }

                    case MessageStatus.OperationUpdateData:
                        {
                            this.logger.Trace($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");

                            this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
                            await this.GetCells();

                            var embarkedLoadingUnit = await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync();

                            lock (this.MachineStatus)
                            {
                                this.MachineStatus.MessageStatus = message.Status;
                                this.MachineStatus.EmbarkedLoadingUnit = embarkedLoadingUnit;
                                this.MachineStatus.EmbarkedLoadingUnitId = embarkedLoadingUnit?.Id;
                            }

                            this.NotifyMachineStatusChanged();
                            break;
                        }

                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationStepStop:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");
                            }

                            this.Loadunits = await this.machineLoadingUnitsWebService.GetAllAsync();
                            await this.GetCells();
                            if (message?.Data is MoveLoadingUnitMessageData)
                            {
                                this.loadUnitMaxHeight = await this.machineLoadingUnitsWebService.GetLoadUnitMaxHeightAsync();
                            }
                            if (this.MachineStatus.IsMovingLoadingUnit)
                            {
                                var missions = await this.missionsWebService.GetAllAsync();
                                this.IsMissionInError = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);

                                this.IsMissionInErrorByLoadUnitOperations = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined && a.MissionType == MAS.AutomationService.Contracts.MissionType.LoadUnitOperation);

                                this.IsMissionWms = missions.Any(a => a.MissionType == MAS.AutomationService.Contracts.MissionType.WMS);
                                this.MachineStatus.IsMovingLoadingUnit = missions.Any(m => m.Status == MAS.AutomationService.Contracts.MissionStatus.Executing);
                            }

                            var embarkedLoadingUnit = await this.GetLodingUnitOnBoardAsync();

                            lock (this.MachineStatus)
                            {
                                this.MachineStatus.MessageStatus = message.Status;

                                if (message.Status == MessageStatus.OperationStop ||
                                    message.Status == MessageStatus.OperationStepStop)
                                {
                                    this.MachineStatus.IsStopped = true;
                                }

                                if (message?.Data is PositioningMessageData dataPositioning)
                                {
                                    this.MachineStatus.EmbarkedLoadingUnit = embarkedLoadingUnit;
                                    this.MachineStatus.EmbarkedLoadingUnitId = embarkedLoadingUnit?.Id;

                                    this.MachineStatus.IsMovingElevator = false;
                                    if (dataPositioning.AxisMovement == Axis.Vertical)
                                    {
                                        this.MachineStatus.VerticalTargetPosition = null;
                                        this.MachineStatus.VerticalSpeed = null;
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                    {
                                        this.MachineStatus.HorizontalTargetPosition = null;
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.BayChain)
                                    {
                                        this.MachineStatus.BayChainTargetPosition = null;
                                    }
                                }

                                this.MachineStatus.IsMoving = false;

                                if (message?.Data is ShutterPositioningMessageData)
                                {
                                    this.MachineStatus.IsMovingShutter = false;
                                }

                                if (message?.Data is MoveLoadingUnitMessageData)
                                {
                                    this.MachineStatus.IsMovingLoadingUnit = false;
                                    this.MachineStatus.CurrentMissionId = null;
                                    this.MachineStatus.CurrentMission = null;
                                    this.MachineStatus.CurrentMissionDescription = null;
                                }

                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; IsMovingLoadingUnit({this.MachineStatus.IsMovingLoadingUnit});");

                                if (!this.MachineStatus.IsMovingLoadingUnit &&
                                    this.sessionService.UserAccessLevel != MAS.AutomationService.Contracts.UserAccessLevel.Operator)
                                {
                                    this.ClearNotifications();
                                }
                            }

                            await this.UpdateLoadUnitInBayAsync();

                            var pos = await this.machineElevatorWebService.GetPositionAsync();
                            if ((!this.MachineStatus.LogicalPositionId.HasValue && !this.MachineStatus.BayPositionId.HasValue)
                                || (this.MachineStatus.LogicalPositionId != pos.CellId && this.MachineStatus.LogicalPositionId != pos.BayPositionId)
                                || this.MachineStatus.ElevatorPositionLoadingUnit is null
                                )
                            {
                                this.UpdateMachineStatusByElevatorPosition(
                                        new ElevatorPositionChangedEventArgs(
                                            pos.Vertical,
                                            pos.Horizontal,
                                            pos.CellId,
                                            pos.BayPositionId,
                                            pos.BayPositionUpper));
                            }
                            else
                            {
                                this.NotifyMachineStatusChanged();
                            }
                            break;
                        }

                    case MessageStatus.OperationError:
                        {
                            if (message?.Data is PositioningMessageData dataLog)
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status}; {dataLog?.AxisMovement};");
                            }
                            else
                            {
                                this.logger.Debug($"OnDataChanged({this.BayNumber}):{typeof(TData).Name}; {message.Status};");
                            }

                            lock (this.MachineStatus)
                            {
                                this.MachineStatus.MessageStatus = message.Status;
                                this.MachineStatus.IsMoving = false;
                                this.MachineStatus.IsError = true;
                                this.MachineStatus.ErrorDescription = message.Description;

                                if (message?.Data is PositioningMessageData dataPositioning)
                                {
                                    this.MachineStatus.IsMovingElevator = false;
                                    if (dataPositioning.AxisMovement == Axis.Vertical)
                                    {
                                        this.MachineStatus.VerticalTargetPosition = null;
                                        this.MachineStatus.VerticalSpeed = null;
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.Horizontal)
                                    {
                                        this.MachineStatus.HorizontalTargetPosition = null;
                                    }
                                    else if (dataPositioning.AxisMovement == Axis.BayChain)
                                    {
                                        this.MachineStatus.BayChainTargetPosition = null;
                                    }
                                }

                                if (message?.Data is ShutterPositioningMessageData)
                                {
                                    this.MachineStatus.IsMovingShutter = false;
                                }

                                if (message?.Data is MoveLoadingUnitMessageData)
                                {
                                    this.MachineStatus.IsMovingLoadingUnit = false;
                                }

                                this.ShowNotification(message.Description, NotificationSeverity.Error);
                            }

                            this.NotifyMachineStatusChanged();
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
            this.UpdateMachineStatusByElevatorPosition(e);
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
                await this.GetLoadUnits();
            }
            catch (Exception exc)
            {
                // do nothing
                this.logger.Debug($"Exception description: {exc.Message}");
            }
        }

        private void OnRepetitiveHorizontalMovementsChanged<TData>(NotificationMessageUI<TData> message)
                                    where TData : class, IMessageData
        {
            try
            {
                if (message?.Data is RepetitiveHorizontalMovementsMessageData dataMessage)
                {
                    this.logger.Debug($"OnRepetitiveHorizontalMovementsChangedAsync({this.BayNumber}):{typeof(TData).Name}; {message.Status};");

                    switch (message.Status)
                    {
                        case MessageStatus.OperationStart:
                            this.MachineStatus.IsDepositAndPickUpRunning = true;
                            break;

                        case MessageStatus.OperationError:
                            this.MachineStatus.IsDepositAndPickUpRunning = false;

                            break;

                        case MessageStatus.OperationEnd:
                        case MessageStatus.OperationStop:
                            this.MachineStatus.IsDepositAndPickUpRunning = false;

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
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
            lock (this.MachineStatus)
            {
                this.MachineStatus.IsMoving = false;
                this.MachineStatus.IsError = false;
                this.MachineStatus.IsMovingElevator = false;
                this.MachineStatus.IsMovingShutter = false;
                this.MachineStatus.IsMovingLoadingUnit = false;
                this.machineStatus.IsDepositAndPickUpRunning = false;
                this.MachineStatus.ErrorDescription = string.Empty;
            }

            this.NotifyMachineStatusChanged();
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
                    async (e) => await this.OnDataChangedAsync(e),
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
                        async (e) => await this.OnDataChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.combinedMovementsChangedToken = this.combinedMovementsChangedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<CombinedMovementsMessageData>>()
                    .Subscribe(
                        async (e) => await this.OnDataChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.repetitiveHorizontalMovementsToken = this.repetitiveHorizontalMovementsToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<RepetitiveHorizontalMovementsMessageData>>()
                    .Subscribe(
                        (e) => this.OnRepetitiveHorizontalMovementsChanged(e),
                        ThreadOption.UIThread,
                        false);

            this.shutterPositionToken = this.shutterPositionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        async (e) => await this.OnDataChangedAsync(e),
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
                        async (e) => await this.OnDataChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.fsmExceptionToken = this.fsmExceptionToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                    .Subscribe(
                        async (e) => await this.OnDataChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.sensorsService.OnUpdateSensors += (s, e) => this.OnChangedEventArgs(e);
        }

        private async Task UpdateBay()
        {
            var missions = (await this.missionsWebService.GetAllAsync());

            this.IsMissionInError = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined);

            this.IsMissionInErrorByLoadUnitOperations = missions.Any(a => a.RestoreStep != MAS.AutomationService.Contracts.MissionStep.NotDefined && a.MissionType == MAS.AutomationService.Contracts.MissionType.LoadUnitOperation);

            this.IsMissionWms = missions.Any(a => a.MissionType == MAS.AutomationService.Contracts.MissionType.WMS);

            this.MachineStatus.IsMovingLoadingUnit = missions.Any(m => m.Status == MAS.AutomationService.Contracts.MissionStatus.Executing);

            // Devo aggiornare i dati delle posizioni della baia
            this.bays = await this.machineBaysWebService.GetAllAsync();
            this.Bay = await this.bayManagerService.GetBayAsync();

            this.HasShutter = (this.Bay.Shutter != null) ? this.Bay.Shutter.Type != ShutterType.NotSpecified : false;
            this.IsShutterThreeSensors = (this.Bay.Shutter != null) ? this.Bay.Shutter.Type is MAS.AutomationService.Contracts.ShutterType.ThreeSensors : false;

            var embarkedLoadingUnit = await this.GetLodingUnitOnBoardAsync();
            var bayChainPosition = await this.machineCarouselWebService.GetPositionAsync();

            lock (this.MachineStatus)
            {
                this.MachineStatus.EmbarkedLoadingUnit = embarkedLoadingUnit;
                this.MachineStatus.EmbarkedLoadingUnitId = embarkedLoadingUnit?.Id;

                this.MachineStatus.BayChainPosition = bayChainPosition;

                if (this.Bay.IsDouble || this.BayFirstPositionIsUpper || this.bay.Carousel != null)
                {
                    if (this.Bay.Positions?.OrderBy(o => o.Height).LastOrDefault() is BayPosition bayPositionUp)
                    {
                        this.MachineStatus.LoadingUnitPositionUpInBay = bayPositionUp.LoadingUnit;
                        if (bayPositionUp.LoadingUnit != null && !this.BayFirstPositionIsUpper)
                        {
                            this.MachineStatus.ElevatorPositionLoadingUnit = bayPositionUp.LoadingUnit;
                        }
                    }
                }

                if (this.Bay.IsDouble || !this.BayFirstPositionIsUpper || this.bay.Carousel != null)
                {
                    if (this.Bay.Positions?.OrderBy(o => o.Height).FirstOrDefault() is BayPosition bayPositionDown)
                    {
                        this.MachineStatus.LoadingUnitPositionDownInBay = bayPositionDown.LoadingUnit;
                        if (bayPositionDown.LoadingUnit != null && this.BayFirstPositionIsUpper)
                        {
                            this.MachineStatus.ElevatorPositionLoadingUnit = bayPositionDown.LoadingUnit;
                        }
                    }
                }
            }

            var pos = await this.machineElevatorWebService.GetPositionAsync();
            if ((!this.MachineStatus.LogicalPositionId.HasValue && !this.MachineStatus.BayPositionId.HasValue)
                || (this.MachineStatus.LogicalPositionId != pos.CellId && this.MachineStatus.LogicalPositionId != pos.BayPositionId)
                || this.MachineStatus.ElevatorPositionLoadingUnit is null
                )
            {
                this.UpdateMachineStatusByElevatorPosition(
                    new ElevatorPositionChangedEventArgs(
                        pos.Vertical,
                        pos.Horizontal,
                        pos.CellId,
                        pos.BayPositionId,
                        pos.BayPositionUpper));
            }
            else
            {
                this.NotifyMachineStatusChanged();
            }
        }

        private void UpdateLoadUnitsId()
        {
            foreach (var cell in this.CellsPlus.Where(c => c.LoadingUnit != null).OrderBy(c => c.Side).ThenBy(c => c.Position))
            {
                foreach (var LU in this.CellsPlus.Where(c => c.Side == cell.Side && c.Position >= cell.Position && c.Position <= cell.Position + cell.LoadingUnit.Height + 12.5))
                {
                    LU.LoadUnitId = cell.LoadingUnit.Id;
                }
            }
        }

        private void UpdateMachineStatusByElevatorPosition(EventArgs e)
        {
            lock (this.MachineStatus)
            {
                if (e is ElevatorPositionChangedEventArgs dataElevatorPosition)
                {
                    if (dataElevatorPosition is null)
                    {
                        return;
                    }

                    this.MachineStatus.ElevatorVerticalPosition = dataElevatorPosition.VerticalPosition;
                    this.MachineStatus.ElevatorHorizontalPosition = dataElevatorPosition.HorizontalPosition;
                    this.MachineStatus.ElevatorPositionType = dataElevatorPosition.ElevatorPositionType;

                    if (dataElevatorPosition.CellId != null)
                    {
                        this.MachineStatus.ElevatorLogicalPosition = string.Format(Resources.Localized.Get("InstallationApp.CellWithNumber"), dataElevatorPosition.CellId);

                        var cell = this.cells?.FirstOrDefault(l => l.Id.Equals(dataElevatorPosition.CellId));
                        if (cell != null)
                        {
                            this.MachineStatus.LogicalPosition = string.Concat(
                                cell?.Side.ToString(),
                                " / ",
                                cell.IsFree ? Resources.Localized.Get("ServiceMachine.CellFree") : Resources.Localized.Get("ServiceMachine.CellBusy"),
                                cell.BlockLevel != BlockLevel.Undefined && cell.BlockLevel != BlockLevel.None ? $" / {cell.BlockLevel}" : string.Empty);
                        }
                        else
                        {
                            this.MachineStatus.LogicalPosition = null;
                        }
                        this.MachineStatus.LogicalPositionId = dataElevatorPosition.CellId;

                        this.MachineStatus.ElevatorPositionLoadingUnit = this.loadingUnits.FirstOrDefault(l => l.CellId.Equals(dataElevatorPosition.CellId));

                        this.MachineStatus.BayPositionUpper = null;
                        this.MachineStatus.BayPositionId = null;
                    }
                    else if (dataElevatorPosition.BayPositionId != null)
                    {
                        var bay = this.bays.SingleOrDefault(b => b.Positions.Any(p => p.Id == dataElevatorPosition.BayPositionId));

                        this.MachineStatus.ElevatorLogicalPosition = string.Format(Resources.Localized.Get("InstallationApp.InBayWithNumber"), (int)bay.Number);

                        if (dataElevatorPosition?.BayPositionUpper ?? false)
                        {
                            this.MachineStatus.LogicalPosition = string.Format(Resources.Localized.Get("ServiceMachine.Position"), Resources.Localized.Get("InstallationApp.PositionOnTop"));
                        }
                        else
                        {
                            this.MachineStatus.LogicalPosition = string.Format(Resources.Localized.Get("ServiceMachine.Position"), Resources.Localized.Get("InstallationApp.PositionOnBotton"));
                        }

                        this.MachineStatus.LogicalPositionId = (int)bay.Number;
                        this.MachineStatus.BayPositionUpper = (dataElevatorPosition?.BayPositionUpper ?? false);
                        this.MachineStatus.BayPositionId = dataElevatorPosition.BayPositionId;

                        var position = this.bay.Positions.SingleOrDefault(p => p.Id == dataElevatorPosition.BayPositionId);
                        this.MachineStatus.ElevatorPositionLoadingUnit = position?.LoadingUnit;
                    }
                    else
                    {
                        this.MachineStatus.ElevatorPositionLoadingUnit = null;
                        this.MachineStatus.ElevatorLogicalPosition = null;
                        this.MachineStatus.LogicalPosition = null;
                        this.MachineStatus.LogicalPositionId = null;
                        this.MachineStatus.BayPositionUpper = null;
                        this.MachineStatus.BayPositionId = null;
                    }
                }

                if (e is BayChainPositionChangedEventArgs dataBayChainPosition)
                {
                    if (this.MachineStatus.BayChainPosition != dataBayChainPosition.Position)
                    {
                        this.MachineStatus.BayChainPosition = dataBayChainPosition.Position;
                    }
                }
            }

            this.NotifyMachineStatusChanged();
        }

        private void WarningsManagement(string view)
        {
            this.ActiveView = view;
            var isWmsHealthy = this.healthProbeService.HealthWmsStatus == HealthStatus.Healthy;

            if (!(view is null) && !this.MachineStatus.IsMoving && !this.MachineStatus.IsMovingLoadingUnit)
            {
                switch (this.GetWarningAreaAttribute())
                {
                    case WarningsArea.None:
                        this.ClearNotifications();
                        break;

                    case WarningsArea.MovementsView:
                    case WarningsArea.Installation:
                        if (this.machineModeService.MachineMode == MachineMode.Shutdown || this.machineModeService.MachineMode == MachineMode.SwitchingToShutdown)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutdownInProgress"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MachineOff"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachineMode < MachineMode.SwitchingToAutomatic &&
                            (this.machineModeService.MachineMode != MachineMode.Manual && this.machineModeService.MachineMode != MachineMode.Manual2 && this.machineModeService.MachineMode != MachineMode.Manual3) &&
                            (this.machineModeService.MachineMode != MachineMode.Test && this.machineModeService.MachineMode != MachineMode.Test2 && this.machineModeService.MachineMode != MachineMode.Test3))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MachineNotManual"), NotificationSeverity.Warning);
                        }
                        else if (this.sensorsService.IsHorizontalInconsistentBothLow &&
                            (this.machineModeService.MachineMode != MachineMode.Test && this.machineModeService.MachineMode != MachineMode.Test2 && this.machineModeService.MachineMode != MachineMode.Test3))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.NoZeroPawlSensor"), NotificationSeverity.Error);
                        }
                        else if (this.sensorsService.IsHorizontalInconsistentBothHigh &&
                            (this.machineModeService.MachineMode != MachineMode.Test && this.machineModeService.MachineMode != MachineMode.Test2 && this.machineModeService.MachineMode != MachineMode.Test3))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.InconsistencyZeroPawlSensor"), NotificationSeverity.Error);
                        }
                        else if ((view.Equals("LoadingUnitFromBayToCellView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("ProfileHeightCheckView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("LoadFirstDrawerView", StringComparison.InvariantCultureIgnoreCase))
                                 && !this.sensorsService.IsLoadingUnitInBay && !this.sensorsService.IsLoadingUnitInMiddleBottomBay)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.NoLoadingUnitInBay"), NotificationSeverity.Warning);
                        }
                        else if (
                            ((this.MachineStatus.EmbarkedLoadingUnitId.GetValueOrDefault() > 0 && (this.sensorsService.IsZeroChain || !this.sensorsService.IsLoadingUnitOnElevator)) ||
                                 (this.MachineStatus.EmbarkedLoadingUnitId.GetValueOrDefault() == 0 && (!this.sensorsService.IsZeroChain || this.sensorsService.IsLoadingUnitOnElevator))
                                 )
                                 && (this.machineModeService.MachineMode != MachineMode.Test && this.machineModeService.MachineMode != MachineMode.Test2 && this.machineModeService.MachineMode != MachineMode.Test3))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.InconsistencyStateAndSensors"), NotificationSeverity.Error);
                        }
                        else if (!this.IsHoming)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.HomingNotPerformed"), NotificationSeverity.Warning);
                        }
                        else if ((((this.MachineStatus.LoadingUnitPositionDownInBay != null && !this.sensorsService.IsLoadingUnitInMiddleBottomBay && (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)) ||
                                   (this.MachineStatus.LoadingUnitPositionUpInBay != null && !this.sensorsService.IsLoadingUnitInBay && (this.Bay.IsDouble || this.BayFirstPositionIsUpper))) ||
                                  ((this.MachineStatus.LoadingUnitPositionDownInBay == null && this.sensorsService.IsLoadingUnitInMiddleBottomBay && (this.Bay.IsDouble || !this.BayFirstPositionIsUpper)) ||
                                   (this.MachineStatus.LoadingUnitPositionUpInBay == null && this.sensorsService.IsLoadingUnitInBay && (this.Bay.IsDouble || this.BayFirstPositionIsUpper)))) &&
                                 !view.Equals("LoadingUnitFromBayToCellView", StringComparison.InvariantCultureIgnoreCase) &&
                                 !view.Equals("ProfileHeightCheckView", StringComparison.InvariantCultureIgnoreCase) &&
                                 !view.Equals("LoadFirstDrawerView", StringComparison.InvariantCultureIgnoreCase) &&
                                 !view.Equals("DepositAndPickUpTestView", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.InconsistencyShutterPresenceSensor"), NotificationSeverity.Error);
                        }
                        else if ((view.Equals("VerticalResolutionCalibrationView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("VerticalOffsetCalibrationView", StringComparison.InvariantCultureIgnoreCase) ||
                                  view.Equals("BayCheckView", StringComparison.InvariantCultureIgnoreCase)) &&
                                 this.sensorsService.IsLoadingUnitOnElevator)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.LoadingUnitOnElevator"), NotificationSeverity.Warning);
                        }
                        // tranne per la macchina con la baia esterna l'elevatore non si può muovere se c'è la serranda aperta
                        else if (!this.bay.IsExternal &&
                                 ((this.bay.Shutter != null) ? this.bay.Shutter.Type != ShutterType.NotSpecified : false) &&
                                 !this.sensorsService.ShutterSensors.Closed && !this.sensorsService.ShutterSensors.MidWay &&
                                 !view.Equals("ProfileHeightCheckView", StringComparison.InvariantCultureIgnoreCase) &&
                                 !view.Equals("HorizontalResolutionCalibrationView", StringComparison.InvariantCultureIgnoreCase) &&
                                 (this.machineModeService.MachineMode != MachineMode.Test && this.machineModeService.MachineMode != MachineMode.Test2 && this.machineModeService.MachineMode != MachineMode.Test3))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutterOpenOrUnknowPosition"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionInError"), NotificationSeverity.Warning);
                        }
                        else if (this.IsWmsEnabled && !isWmsHealthy)
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.WmsOffline"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionWms && !this.IsWmsEnabled)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionsWmsNotActive"), NotificationSeverity.Warning);
                        }
                        else if (view.Equals("CarouselCalibrationView", StringComparison.InvariantCultureIgnoreCase) &&
                                !this.sensorsService.BayZeroChain)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.CalibrationCarouselFailedChainNotZeroPosition"), NotificationSeverity.Warning);
                        }
                        else if (!this.isBayHoming[this.bay.Number] &&
                                 !view.Equals("HorizontalResolutionCalibrationView", StringComparison.InvariantCultureIgnoreCase) &&
                                 !view.Equals("DepositAndPickUpTestView", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.BayCalibrationNotPerformed"), NotificationSeverity.Warning);
                        }
                        else if (this.bay.Positions.All(p => p.IsBlocked) &&
                                 !view.Equals("DepositAndPickUpTestView", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.BayPositionsDisabled"), NotificationSeverity.Warning);
                        }
                        else if (this.MachineStatus.EmbarkedLoadingUnitId.GetValueOrDefault() > 0
                            && this.MachineStatus.EmbarkedLoadingUnit.Height == 0
                            )
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.LoadUnitOnBoardHasInvalidHeight"), NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.Picking:
                        if (this.machineModeService.MachineMode == MachineMode.Shutdown || this.machineModeService.MachineMode == MachineMode.SwitchingToShutdown)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutdownInProgress"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MachineOff"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachineMode != MachineMode.Automatic
                            && this.machineModeService.MachineMode != MachineMode.SwitchingToAutomatic
                            )
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.AutomaticMissing"), NotificationSeverity.Warning);
                        }
                        else if (this.IsWmsEnabled && !isWmsHealthy)
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.WmsOffline"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionWms && !this.IsWmsEnabled)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionsWmsNotActive"), NotificationSeverity.Warning);
                        }
                        else if (this.IsSensorMissing())
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.LoadUnitInBaySensorMissing"), NotificationSeverity.Warning);
                        }
                        else if (this.bay.Positions.All(p => p.IsBlocked))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.BayPositionsDisabled"), NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.User:
                        if (this.machineModeService.MachineMode == MachineMode.Shutdown || this.machineModeService.MachineMode == MachineMode.SwitchingToShutdown)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutdownInProgress"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MachineOff"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachineMode != MachineMode.Automatic
                            && this.machineModeService.MachineMode != MachineMode.SwitchingToAutomatic
                            )
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.AutomaticMissing"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionInError"), NotificationSeverity.Warning);
                        }
                        else if (this.IsWmsEnabled && !isWmsHealthy)
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.WmsOffline"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionWms && !this.IsWmsEnabled)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionsWmsNotActive"), NotificationSeverity.Warning);
                        }
                        else if (this.isHomingStarted[Axis.Horizontal])
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.HorizontalHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.isHomingStarted[Axis.Vertical] || this.isHomingStarted[Axis.HorizontalAndVertical])
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.VerticalHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.isHomingStarted[Axis.BayChain])
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.BayHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.IsSensorMissing())
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.LoadUnitInBaySensorMissing"), NotificationSeverity.Warning);
                        }
                        else if (!this.isBayHoming[this.bay.Number])
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.BayCalibrationNotPerformed"), NotificationSeverity.Warning);
                        }
                        else if (this.bay.Positions.All(p => p.IsBlocked))
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.BayPositionsDisabled"), NotificationSeverity.Warning);
                        }
                        else if (this.FreeSpaceFront < this.loadUnitMaxHeight && this.FreeSpaceBack < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.InsufficientSpaceAvailable"), NotificationSeverity.Warning);
                        }
                        else if (this.MaxSolidSpace < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        }
                        //else if (this.FragmentTotalPercent > MaximumFragmentation)
                        //{
                        //    this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        //}
                        else if (this.IsAnyExpired)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expired"), NotificationSeverity.Warning);
                        }
                        else if (this.IsAnyExpiring)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expiring"), NotificationSeverity.Warning);
                        }
                        else
                        {
                            this.ClearNotifications();
                        }
                        break;

                    case WarningsArea.Maintenance:
                        if (this.machineModeService.MachineMode == MachineMode.Shutdown || this.machineModeService.MachineMode == MachineMode.SwitchingToShutdown)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutdownInProgress"), NotificationSeverity.Warning);
                        }
                        else if (this.machineModeService.MachinePower != MachinePowerState.Powered)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MachineOff"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionInError"), NotificationSeverity.Warning);
                        }
                        else if (this.IsWmsEnabled && !isWmsHealthy)
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.WmsOffline"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionWms && !this.IsWmsEnabled)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionsWmsNotActive"), NotificationSeverity.Warning);
                        }
                        else if (view.Equals("DrawerCompactingView", StringComparison.InvariantCultureIgnoreCase) &&
                                !this.MachineStatus.IsMovingLoadingUnit &&
                                !this.sensorsService.IsZeroChain)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.LoadingUnitOnElevator"), NotificationSeverity.Error);
                        }
                        else if (this.FreeSpaceFront < this.loadUnitMaxHeight && this.FreeSpaceBack < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.InsufficientSpaceAvailable"), NotificationSeverity.Warning);
                        }
                        else if (this.MaxSolidSpace < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        }
                        //else if (this.FragmentTotalPercent > MaximumFragmentation)
                        //{
                        //    this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        //}
                        else if (this.IsAnyExpired)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expired"), NotificationSeverity.Warning);
                        }
                        else if (this.IsAnyExpiring)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expiring"), NotificationSeverity.Warning);
                        }
                        break;

                    case WarningsArea.Information:
                    case WarningsArea.Menu:
                        if (this.machineModeService.MachineMode == MachineMode.Shutdown || this.machineModeService.MachineMode == MachineMode.SwitchingToShutdown)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.ShutdownInProgress"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionInError)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionInError"), NotificationSeverity.Warning);
                        }
                        else if (this.IsWmsEnabled && !isWmsHealthy)
                        {
                            this.ShowNotification(Resources.Localized.Get("InstallationApp.WmsOffline"), NotificationSeverity.Warning);
                        }
                        else if (this.IsMissionWms && !this.IsWmsEnabled)
                        {
                            this.ShowNotification(Resources.Localized.Get("ServiceMachine.MissionsWmsNotActive"), NotificationSeverity.Warning);
                        }
                        else if (this.isHomingStarted[Axis.Horizontal])
                        {
                            this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.HorizontalHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.isHomingStarted[Axis.Vertical] || this.isHomingStarted[Axis.HorizontalAndVertical])
                        {
                            this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.VerticalHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.isHomingStarted[Axis.BayChain])
                        {
                            this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.BayHomingStarted"), NotificationSeverity.Info);
                        }
                        else if (this.FreeSpaceFront < this.loadUnitMaxHeight && this.FreeSpaceBack < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.InsufficientSpaceAvailable"), NotificationSeverity.Warning);
                        }
                        else if (this.MaxSolidSpace < this.loadUnitMaxHeight)
                        {
                            this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        }
                        //else if (this.FragmentTotalPercent > MaximumFragmentation)
                        //{
                        //    this.ShowNotification(Resources.Localized.Get("OperatorApp.DrawerCompactingWarning"), NotificationSeverity.Warning);
                        //}
                        else if (this.IsAnyExpired)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expired"), NotificationSeverity.Warning);
                        }
                        else if (this.IsAnyExpiring)
                        {
                            this.ShowNotification(Resources.Localized.Get("MaintenanceMenu.Expiring"), NotificationSeverity.Warning);
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
            else if (!(view is null) && this.MachineStatus.IsMovingLoadingUnit)
            {
                if (this.sessionService.UserAccessLevel == MAS.AutomationService.Contracts.UserAccessLevel.Operator)
                {
                    switch (this.GetWarningAreaAttribute())
                    {
                        case WarningsArea.User:
                            if (this.MachineStatus.CurrentMission != null)
                            {
                                //switch (this.MachineStatus.CurrentMission.MissionStep)
                                switch (this.MachineStatus.CurrentMissionDescription)
                                {
                                    //case CommonUtils.Messages.Enumerations.MissionStep.DepositUnit:
                                    case "DepositUnit":
                                    case "LoadElevator":
                                        this.ShowNotification(Resources.Localized.Get("ServiceMachine.MovementLoadingUnitInProgress"), NotificationSeverity.Info);
                                        break;

                                    case "BackToTarget":
                                    case "ToTarget":
                                        this.ShowNotification(Resources.Localized.Get("ServiceMachine.MovementElevatorInProgress"), NotificationSeverity.Info);
                                        break;

                                    case "WaitPick":
                                        this.ShowNotification(Resources.Localized.Get("ServiceMachine.LoadingUnitReady"), NotificationSeverity.Warning);
                                        break;

                                    case "BayChain":
                                        this.ShowNotification(Resources.Localized.Get("ServiceMachine.MovementBayChainInProgress"), NotificationSeverity.Info);
                                        break;

                                    case "CloseShutter":
                                        this.ShowNotification(Resources.Localized.Get("ServiceMachine.MovementShutterInProgress"), NotificationSeverity.Info);
                                        break;

                                    case "Error":
                                    case "ErrorLoad":
                                    case "ErrorDeposit":
                                        this.ShowNotification("Error.", NotificationSeverity.Error);
                                        break;
                                }
                            }
                            break;
                    }
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
                view == WarningsArea.Picking ||
                view == WarningsArea.User)
            {
                return;
            }

            if (this.MachineStatus.IsMovingShutter)
            {
                if (this.machineModeService.MachineMode == MachineMode.Test || this.machineModeService.MachineMode == MachineMode.Test2 || this.machineModeService.MachineMode == MachineMode.Test3)
                {
                    this.Notification = Resources.Localized.Get("ServiceMachine.TestInProgress");
                }
                else
                {
                    this.Notification = Resources.Localized.Get("ServiceMachine.MovementShutterInProgress");
                }
            }
            else
            {
                if (this.machineModeService.MachineMode == MachineMode.Manual || this.machineModeService.MachineMode == MachineMode.Manual2 || this.machineModeService.MachineMode == MachineMode.Manual3)
                {
                    if (axisMovement.HasValue && axisMovement == Axis.Vertical)
                    {
                        this.Notification = Resources.Localized.Get("ServiceMachine.MovementVerticalInProgress");
                    }
                    else if (axisMovement.HasValue && axisMovement == Axis.Horizontal)
                    {
                        this.Notification = Resources.Localized.Get("ServiceMachine.MovementHorizontalInProgress");
                    }
                    else if (axisMovement.HasValue && axisMovement == Axis.BayChain)
                    {
                        this.Notification = Resources.Localized.Get("ServiceMachine.MovementBayChainInProgress");
                    }
                }
                else if (this.machineModeService.MachineMode == MachineMode.Test || this.machineModeService.MachineMode == MachineMode.Test2 || this.machineModeService.MachineMode == MachineMode.Test3)
                {
                    this.Notification = Resources.Localized.Get("ServiceMachine.TestInProgress");
                }
                else
                {
                    this.Notification = string.Format(Resources.Localized.Get("ServiceMachine.MovementInProgress"), "");
                }
            }
        }

        #endregion
    }
}
