using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

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

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private bool bay1IsVisible;

        private bool bay1ZeroChainisVisible;

        private bool bay2IsVisible;

        private bool bay2ZeroChainisVisible;

        private bool bay3IsVisible;

        private bool bay3ZeroChainisVisible;

        private double? bayChainPosition;

        private SubscriptionToken bayChainPositionChangedToken;

        private bool bayIsMultiPosition;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private double? bayPositionDownHeight;

        private double? bayPositionUpHeight;

        private int? elevatorBayPositionId;

        private int? elevatorCellId;

        private double? elevatorHorizontalPosition;

        private string elevatorLogicalPosition;

        private SubscriptionToken elevatorPositionChangedToken;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool isShutterThreeSensors;

        private string loadingUnitPositionDownInBayCode;

        private string loadingUnitPositionUpInBayCode;

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

            this.SubscribeToEvents();
        }

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

        public bool Bay1IsVisible
        {
            get => this.bay1IsVisible;
            set => this.SetProperty(ref this.bay1IsVisible, value);
        }

        public bool Bay1ZeroChainIsVisible
        {
            get => this.bay1ZeroChainisVisible;
            private set => this.SetProperty(ref this.bay1ZeroChainisVisible, value);
        }

        public bool Bay2IsVisible
        {
            get => this.bay2IsVisible;
            set => this.SetProperty(ref this.bay2IsVisible, value);
        }

        public bool Bay2ZeroChainIsVisible { get => this.bay2ZeroChainisVisible; private set => this.SetProperty(ref this.bay2ZeroChainisVisible, value); }

        public bool Bay3IsVisible
        {
            get => this.bay3IsVisible;
            set => this.SetProperty(ref this.bay3IsVisible, value);
        }

        public bool Bay3ZeroChainIsVisible { get => this.bay3ZeroChainisVisible; private set => this.SetProperty(ref this.bay3ZeroChainisVisible, value); }

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

        public bool IsLoadingUnitInBay
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.LUPresentInBay1;
                }
                else if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.LUPresentInBay2;
                }
                else if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.LUPresentInBay3;
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
                this.RetrieveElevatorPosition();

                await this.GetBayAsync();

                this.GetShutter();

                await this.CheckZeroChainOnBays();

                await this.InitializeSensorsAsync();

                await this.GetElevatorAsync(forceRefresh);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task CheckZeroChainOnBays()
        {
            var bays = await this.machineBaysWebService.GetAllAsync();

            this.Bay1ZeroChainIsVisible = bays
                  .Where(b => b.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
                  .Select(b => b.Carousel != null || b.IsExternal)
                  .SingleOrDefault() && this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne;

            this.Bay2ZeroChainIsVisible = bays
                  .Where(b => b.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
                  .Select(b => b.Carousel != null || b.IsExternal)
                  .SingleOrDefault() && this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo;

            this.Bay3ZeroChainIsVisible = bays
                  .Where(b => b.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
                  .Select(b => b.Carousel != null || b.IsExternal)
                  .SingleOrDefault() && this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree;
        }

        private async Task GetBayAsync()
        {
            try
            {
                if (this.IsHealthy)
                {
                    this.Bay = await this.bayManagerService.GetBayAsync();
                    this.BayNumber = this.Bay.Number;

                    this.Bay1IsVisible = this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne;
                    this.Bay2IsVisible = this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo;
                    this.Bay3IsVisible = this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree;

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
            this.shutterSensors = new ShutterSensors((int)this.Bay.Number);
        }

        private async Task InitializeSensorsAsync()
        {
            var sensorsStates = await this.machineSensorsWebService.GetAsync();

            this.sensors.Update(sensorsStates.ToArray());
            this.shutterSensors.Update(sensorsStates.ToArray());

            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));

            this.RaisePropertyChanged();
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.BayChainPosition = e.Position;
        }

        private async Task OnElevatorPositionChangedAsync(ElevatorPositionChangedEventArgs e)
        {
            this.ElevatorVerticalPosition = e.VerticalPosition;
            this.ElevatorHorizontalPosition = e.HorizontalPosition;

            if (e.CellId != null && this.elevatorCellId != e.CellId)
            {
                this.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.CellWithNumber, e.CellId);
                this.elevatorCellId = e.CellId;
            }
            else if (e.BayPositionId != null && this.elevatorBayPositionId != e.BayPositionId)
            {
                this.elevatorBayPositionId = e.BayPositionId;

                try
                {
                    var bays = await this.machineBaysWebService.GetAllAsync();
                    var bay = bays.SingleOrDefault(b => b.Positions.Any(p => p.Id == e.BayPositionId));
                    this.ElevatorLogicalPosition = string.Format(Resources.InstallationApp.InBayWithNumber, (int)bay.Number);
                }
                catch
                {
                    this.ElevatorLogicalPosition = Resources.InstallationApp.InBay;
                }
            }
            else
            {
                this.ElevatorLogicalPosition = null;
            }
        }

        private async Task OnPositioningOperationChangedAsync(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationEnd when message.Data.AxisMovement is Axis.Horizontal:
                    {
                        await this.GetElevatorAsync(false);
                        await this.GetElevatorAsync(false);

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
                this.shutterSensors?.Update(message.Data.SensorsStates);
            }

            await this.GetBayAsync();

            await this.GetElevatorAsync(false);

            this.RaisePropertyChanged();
        }

        private void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.IsZeroChain));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
        }

        private void RetrieveElevatorPosition()
        {
            this.ElevatorVerticalPosition = this.machineElevatorService.Position?.Vertical;
            this.ElevatorHorizontalPosition = this.machineElevatorService.Position?.Horizontal;
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
                        async m => await this.OnElevatorPositionChangedAsync(m),
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
