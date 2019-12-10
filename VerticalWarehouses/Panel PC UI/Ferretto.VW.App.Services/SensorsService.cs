using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Data.Native;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using ShutterType = Ferretto.VW.MAS.AutomationService.Contracts.ShutterType;

namespace Ferretto.VW.App.Services
{
    public class SensorsService : BindableBase, ISensorsService
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private double? bayChainPosition;

        private SubscriptionToken bayChainPositionChangedToken;

        private bool bayIsMultiPosition;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private double? bayPositionDownHeight;

        private double? bayPositionUpHeight;

        private IEnumerable<Bay> bays;

        private bool bayZeroChainIsVisible;

        private double? elevatorHorizontalPosition;

        private string elevatorLogicalPosition;

        private SubscriptionToken elevatorPositionChangedToken;

        private LoadingUnit elevatorPositionLoadingUnit;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool hasShutter;

        private bool isShutterThreeSensors;

        private string loadingUnitPositionDownInBayCode;

        private string loadingUnitPositionUpInBayCode;

        private IEnumerable<LoadingUnit> loadingUnits;

        private string logicalPosition;

        private string logicalPositionId;

        private SubscriptionToken positioningOperationChangedToken;

        private SubscriptionToken sensorsToken;

        private ShutterSensors shutterSensors;

        #endregion

        #region Constructors

        public SensorsService(
            IMachineBaysWebService machineBaysWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IHealthProbeService healthProbeService,
            IEventAggregator eventAggregator,
            IMachineElevatorService machineElevatorService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IBayManager bayManagerService)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));

            this.SubscribeToEvents();
        }

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

        public double? BayChainPosition
        {
            get => this.bayChainPosition;
            private set => this.SetProperty(ref this.bayChainPosition, value);
        }

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public MAS.AutomationService.Contracts.BayNumber BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value);
        }

        public double? BayPositionDownHeight
        {
            get => this.bayPositionDownHeight;
            private set => this.SetProperty(ref this.bayPositionDownHeight, value);
        }

        public double? BayPositionUpHeight
        {
            get => this.bayPositionUpHeight;
            private set => this.SetProperty(ref this.bayPositionUpHeight, value);
        }

        public bool BayZeroChain
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.ACUBay1S3IND;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.ACUBay2S3IND;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.ACUBay3S3IND;
                }

                return false;
            }
        }

        public bool BayZeroChainIsVisible
        {
            get => this.bayZeroChainIsVisible;
            set => this.SetProperty(ref this.bayZeroChainIsVisible, value);
        }

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public string ElevatorLogicalPosition
        {
            get => this.elevatorLogicalPosition;
            private set => this.SetProperty(ref this.elevatorLogicalPosition, value);
        }

        public LoadingUnit ElevatorPositionLoadingUnit
        {
            get => this.elevatorPositionLoadingUnit;
            private set => this.SetProperty(ref this.elevatorPositionLoadingUnit, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

        public LoadingUnit EmbarkedLoadingUnit
        {
            get => this.embarkedLoadingUnit;
            private set => this.SetProperty(ref this.embarkedLoadingUnit, value);
        }

        public bool HasShutter
        {
            get => this.hasShutter;
            set => this.SetProperty(ref this.hasShutter, value);
        }

        public bool IsLoadingUnitInBay
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.LUPresentInBay1;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.LUPresentInBay2;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.LUPresentInBay3;
                }

                return false;
            }
        }

        public bool IsLoadingUnitInMiddleBottomBay
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.LUPresentMiddleBottomBay1;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.LUPresentMiddleBottomBay2;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.LUPresentMiddleBottomBay3;
                }

                return false;
            }
        }

        public bool IsLoadingUnitOnElevator => this.Sensors.LuPresentInMachineSide && this.Sensors.LuPresentInOperatorSide;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsShutterThreeSensors
        {
            get => this.isShutterThreeSensors;
            set => this.SetProperty(ref this.isShutterThreeSensors, value);
        }

        public bool IsZeroChain => this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;

        public string LoadingUnitPositionDownInBayCode
        {
            get => this.loadingUnitPositionDownInBayCode;
            private set => this.SetProperty(ref this.loadingUnitPositionDownInBayCode, value);
        }

        public string LoadingUnitPositionUpInBayCode
        {
            get => this.loadingUnitPositionUpInBayCode;
            private set => this.SetProperty(ref this.loadingUnitPositionUpInBayCode, value);
        }

        public string LogicalPosition
        {
            get => this.logicalPosition;
            private set => this.SetProperty(ref this.logicalPosition, value);
        }

        public string LogicalPositionId
        {
            get => this.logicalPositionId;
            private set => this.SetProperty(ref this.logicalPositionId, value);
        }

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

        private bool IsHealthy => this.healthProbeService?.HealthStatus == HealthStatus.Healthy;

        #endregion

        #region Methods

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public async Task RefreshAsync(bool forceRefresh)
        {
            try
            {
                this.bays = await this.machineBaysWebService.GetAllAsync();

                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();

                this.RetrieveElevatorPosition(this.machineElevatorService.Position);

                await this.GetBayAsync()
                    .ContinueWith(async (m) => await this.InitializeSensorsAsync());

                this.GetShutter();

                await this.GetElevatorAsync(forceRefresh);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public void RetrieveElevatorPosition(ElevatorPosition position)
        {
            if (position is null)
            {
                return;
            }

            this.ElevatorVerticalPosition = position.Vertical;
            this.ElevatorHorizontalPosition = position.Horizontal;

            if (position.CellId != null)
            {
                this.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.CellWithNumber, position.CellId);
                this.LogicalPosition = Resources.InstallationApp.Cell;
                this.LogicalPositionId = position.CellId.ToString();
                this.ElevatorPositionLoadingUnit = this.loadingUnits?.FirstOrDefault(l => l.CellId.Equals(position.CellId));
            }
            else if (position.BayPositionId != null)
            {
                if (this.bays != null)
                {
                    var bay = this.bays.SingleOrDefault(b => b.Positions.Any(p => p.Id == position.BayPositionId));
                    System.Diagnostics.Debug.Assert(bay != null);
                    this.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.InBayWithNumber, (int)bay.Number);
                    this.LogicalPosition = Resources.InstallationApp.InBay;
                    this.LogicalPositionId = ((int)bay.Number).ToString();
                    this.ElevatorPositionLoadingUnit = this.EmbarkedLoadingUnit;
                }
                else
                {
                    this.ElevatorLogicalPosition = Resources.InstallationApp.InBay;
                    this.LogicalPosition = Resources.InstallationApp.InBay;
                    this.LogicalPositionId = null;
                    this.ElevatorPositionLoadingUnit = null;
                }
            }
            else
            {
                this.ElevatorPositionLoadingUnit = null;
                this.ElevatorLogicalPosition = null;
                this.LogicalPosition = null;
                this.LogicalPositionId = null;
            }
        }

        private async Task GetBayAsync()
        {
            try
            {
                if (this.IsHealthy)
                {
                    this.Bay = await this.bayManagerService.GetBayAsync();
                    this.BayNumber = this.Bay.Number;

                    if (this.Bay.Positions?.FirstOrDefault() is BayPosition bayPositionDown)
                    {
                        this.BayPositionDownHeight = bayPositionDown.Height;
                        this.LoadingUnitPositionDownInBayCode = bayPositionDown.LoadingUnit?.Id.ToString();
                    }

                    if (this.Bay.Positions?.LastOrDefault() is BayPosition bayPositionUp)
                    {
                        this.LoadingUnitPositionUpInBayCode = bayPositionUp.LoadingUnit?.Id.ToString();
                        this.BayPositionUpHeight = bayPositionUp.Height;
                    }

                    this.BayIsMultiPosition = this.Bay.IsDouble;

                    this.HasShutter = this.Bay.Shutter.Type != ShutterType.NotSpecified;

                    this.BayZeroChainIsVisible = this.Bay.IsExternal || this.Bay.Carousel != null;

                    this.BayChainPosition = await this.machineCarouselWebService.GetPositionAsync();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task GetElevatorAsync(bool forceRefresh)
        {
            try
            {
                if (this.IsHealthy)
                {
                    var position = await this.machineElevatorWebService.GetPositionAsync();
                    this.ElevatorVerticalPosition = position.Vertical;
                    this.ElevatorHorizontalPosition = position.Horizontal;

                    var isLoadingUnitEmbarked =
                        this.sensors.LuPresentInMachineSide
                        &&
                        this.sensors.LuPresentInOperatorSide;

                    this.EmbarkedLoadingUnit = isLoadingUnitEmbarked || forceRefresh
                        ? await this.machineElevatorWebService.GetLoadingUnitOnBoardAsync()
                        : null;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void GetShutter()
        {
            this.IsShutterThreeSensors = this.Bay.Shutter.Type is MAS.AutomationService.Contracts.ShutterType.ThreeSensors;
        }

        private async Task InitializeSensorsAsync()
        {
            var sensorsStates = await this.machineSensorsWebService.GetAsync();

            this.sensors.Update(sensorsStates.ToArray());

            if (this.shutterSensors is null)
            {
                this.shutterSensors = new ShutterSensors((int)this.Bay.Number);
            }
            this.shutterSensors.Update(sensorsStates.ToArray());

            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));

            this.RaisePropertyChanged();
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.BayChainPosition = e.Position;
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.RetrieveElevatorPosition(
                new ElevatorPosition
                {
                    Horizontal = e.HorizontalPosition,
                    Vertical = e.VerticalPosition,
                    BayPositionId = e.BayPositionId,
                    CellId = e.CellId
                });
        }

        private async Task OnPositioningOperationChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationEnd when message.Data.AxisMovement is Axis.Horizontal:
                    {
                        await this.GetElevatorAsync(false);
                        break;
                    }
                case MessageStatus.OperationUpdateData:
                    {
                        await this.GetElevatorAsync(true);
                        break;
                    }
                default:
                    {
                        this.RaisePropertyChanged();
                        break;
                    }
            }
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (message?.Data?.SensorsStates != null)
            {
                this.sensors.Update(message.Data.SensorsStates);
                if (this.shutterSensors is null)
                {
                    await this.GetBayAsync()
                        .ContinueWith(m =>
                        {
                            if (this.Bay != null)
                            {
                                this.shutterSensors = new ShutterSensors((int)this.Bay.Number);
                                this.shutterSensors.Update(message.Data.SensorsStates);
                            }
                        });
                }
                else
                {
                    this.shutterSensors.Update(message.Data.SensorsStates);
                }
            }

            await this.GetElevatorAsync(false);

            this.RaisePropertyChanged();
        }

        private void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.IsZeroChain));
            this.RaisePropertyChanged(nameof(this.BayZeroChain));
            this.RaisePropertyChanged(nameof(this.BayZeroChainIsVisible));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInMiddleBottomBay));
        }

        private void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
        }

        private void SubscribeToEvents()
        {
            this.sensorsToken = this.sensorsToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        async m => await this.OnSensorsChangedAsync(m),
                        ThreadOption.UIThread,
                        false,
                        m => m?.Data != null);

            this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
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

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        async m => await this.OnPositioningOperationChangedAsync(m),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
