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

namespace Ferretto.VW.App.Services
{
    public class SensorsService : BindableBase, ISensorsService
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private bool bay1ZeroChainisVisible;

        private bool bay2IsVisible;

        private bool bay2ZeroChainisVisible;

        private bool bay3IsVisible;

        private bool bay3ZeroChainisVisible;

        private double? bayChainPosition;

        private bool bayIsMultiPosition;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private double? bayPosition1Height;

        private double? bayPosition2Height;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool isShutterTwoSensors;

        private LoadingUnit loadingUnitPosition1InBay;

        private LoadingUnit loadingUnitPosition2InBay;

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
         IEventAggregator eventAggregator,
         IBayManager bayManagerService)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));

            this.Initialize();
        }

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

        public bool Bay1IsVisible
        {
            get => this.bay3IsVisible;
            set => this.SetProperty(ref this.bay3IsVisible, value);
        }

        public bool Bay1ZeroChainIsVisible { get => this.bay1ZeroChainisVisible; private set => this.SetProperty(ref this.bay1ZeroChainisVisible, value); }

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

        public double? BayPosition1Height
        {
            get => this.bayPosition1Height;
            private set => this.SetProperty(ref this.bayPosition1Height, value);
        }

        public double? BayPosition2Height
        {
            get => this.bayPosition2Height;
            private set => this.SetProperty(ref this.bayPosition2Height, value);
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
            // TODO  for the moment we use only presence sensors
            // get => this.embarkedLoadingUnit;
            get
            {
                if (this.CanEmbark())
                {
                    this.embarkedLoadingUnit = new LoadingUnit();
                }
                else
                {
                    this.embarkedLoadingUnit = null;
                }

                return this.embarkedLoadingUnit;
            }

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

        public bool IsLoadingUnitOnElevator => this.Sensors.LuPresentInMachineSideBay1 && this.Sensors.LuPresentInOperatorSideBay1;

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsShutterTwoSensors
        {
            get => this.isShutterTwoSensors;
            set => this.SetProperty(ref this.isShutterTwoSensors, value);
        }

        public bool IsZeroChain => this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;

        public LoadingUnit LoadingUnitPosition1InBay
        {
            get => this.loadingUnitPosition1InBay;
            private set => this.SetProperty(ref this.loadingUnitPosition1InBay, value);
        }

        public LoadingUnit LoadingUnitPosition2InBay
        {
            get => this.loadingUnitPosition2InBay;
            private set => this.SetProperty(ref this.loadingUnitPosition2InBay, value);
        }

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

        #endregion

        #region Methods

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public async Task RefreshAsync()
        {
            try
            {
                await this.RetrieveElevatorPositionAsync();

                await this.GetBayAsync();

                this.GetShutter();

                await this.CheckZeroChainOnBays();

                await this.InitializeSensors();
            }
            catch
            {
            }
        }

        private bool CanEmbark()
        {
            return
                !this.sensors.LuPresentInMachineSideBay1
                &&
                !this.sensors.LuPresentInOperatorSideBay1
                &&
                this.IsZeroChain;
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
            this.LoadingUnitPosition1InBay = null;
            this.LoadingUnitPosition2InBay = null;

            this.Bay = await this.bayManagerService.GetBayAsync();
            this.BayNumber = this.Bay.Number;

            this.Bay1IsVisible = (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne);
            this.Bay2IsVisible = (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo);
            this.Bay3IsVisible = (this.BayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree);

            if (this.Bay.Positions?.FirstOrDefault() is BayPosition bayPosition1)
            {
                this.BayPosition1Height = bayPosition1.Height;
                this.LoadingUnitPosition1InBay = bayPosition1.LoadingUnit;
            }

            if (this.Bay.Positions?.LastOrDefault() is BayPosition bayPosition2)
            {
                this.LoadingUnitPosition2InBay = bayPosition2.LoadingUnit;
                this.BayPosition2Height = bayPosition2.Height;
            }

            this.BayIsMultiPosition = this.Bay.IsDouble;

            this.BayChainPosition = await this.machineCarouselWebService.GetPositionAsync();
        }

        private void GetShutter()
        {
            this.IsShutterTwoSensors = this.Bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.TwoSensors;

            this.shutterSensors = new ShutterSensors((int)this.Bay.Number);
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
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.RefreshAsync();
        }

        private async Task InitializeSensors()
        {
            var sensorsStates = await this.machineSensorsWebService.GetAsync();

            this.sensors.Update(sensorsStates.ToArray());
            this.shutterSensors.Update(sensorsStates.ToArray());

            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));

            this.RaisePropertyChanged();
        }

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
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

                        break;
                    }
            }
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
            this.shutterSensors.Update(message.Data.SensorsStates);

            await this.GetBayAsync();

            this.RaisePropertyChanged();
        }

        private void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.IsZeroChain));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
            this.RaisePropertyChanged(nameof(this.EmbarkedLoadingUnit));
        }

        private async Task RetrieveElevatorPositionAsync()
        {
            this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
            this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
        }

        #endregion
    }
}
