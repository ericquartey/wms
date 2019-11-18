using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
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

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private double? bayChainPosition;

        private bool bayIsMultiPosition;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private double? bayPositionDownHeight;

        private double? bayPositionUpHeight;

        private bool bayZeroChainIsVisible;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool hasShutter;

        private bool isShutterThreeSensors;

        private string loadingUnitPositionDownInBayCode;

        private string loadingUnitPositionUpInBayCode;

        private SubscriptionToken positioningToken;

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
            IBayManager bayManagerService)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.Initialize();
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
                await this.RetrieveElevatorPositionAsync();

                await this.GetBayAsync();

                this.GetShutter();

                await this.InitializeSensors();

                await this.GetElevatorAsync(forceRefresh);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
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
                        this.LoadingUnitPositionDownInBayCode = bayPositionDown.LoadingUnit?.Code;
                    }

                    if (this.Bay.Positions?.LastOrDefault() is BayPosition bayPositionUp)
                    {
                        this.LoadingUnitPositionUpInBayCode = bayPositionUp.LoadingUnit?.Code;
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
                    this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                    this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();

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
            this.IsShutterThreeSensors = this.Bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.ThreeSensors;
        }

        private void Initialize()
        {
            this.sensorsToken = this.sensorsToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        (async (m) => await this.OnSensorsChangedAsync(m)),
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            this.positioningToken = this.positioningToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        async (m) => await this.OnElevatorPositionChangedAsync(m),
                        ThreadOption.UIThread,
                        false);
        }

        private async Task InitializeSensors()
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

        private async Task OnElevatorPositionChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationExecuting:
                    {
                        if (message.Data.AxisMovement == Axis.Vertical)
                        {
                            this.ElevatorVerticalPosition = message?.Data?.CurrentPosition ?? this.ElevatorVerticalPosition;
                        }
                        else if (message.Data.AxisMovement == Axis.Horizontal)
                        {
                            this.ElevatorHorizontalPosition = message?.Data?.CurrentPosition ?? this.ElevatorHorizontalPosition;
                        }
                        else if (message.Data.AxisMovement == Axis.BayChain)
                        {
                            this.BayChainPosition = message?.Data?.CurrentPosition ?? this.BayChainPosition;
                        }

                        break;
                    }
                case MessageStatus.OperationEnd:
                    {
                        if (message.Data.AxisMovement == Axis.Horizontal)
                        {
                            await this.GetElevatorAsync(false);
                            //leggo l'altezza
                        }

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
            this.sensors.Update(message.Data.SensorsStates);

            if (this.shutterSensors is null)
            {
                this.shutterSensors = new ShutterSensors((int)this.Bay.Number);
            }
            this.shutterSensors.Update(message.Data.SensorsStates);

            await this.GetBayAsync();

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

        private async Task RetrieveElevatorPositionAsync()
        {
            this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
            this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
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

        #endregion
    }
}
