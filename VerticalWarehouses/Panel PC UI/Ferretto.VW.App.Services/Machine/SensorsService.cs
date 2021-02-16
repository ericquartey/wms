using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
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

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private readonly ShutterSensors shutterSensors = new ShutterSensors();

        private bool bayIsMultiPosition;

        private MAS.AutomationService.Contracts.BayNumber bayNumber;

        private double? bayPositionDownHeight;

        private double? bayPositionUpHeight;

        private bool bayZeroChainIsVisible;

        private SubscriptionToken healthProbeToken;

        private SubscriptionToken sensorsToken;

        #endregion

        #region Constructors

        public SensorsService(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IEventAggregator eventAggregator,
            IBayManager bayManagerService)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.SubscribeToEvents();
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> OnUpdateSensors;

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

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

        public bool BayTrolleyOption
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.TrolleyOptionBay1;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.TrolleyOptionBay2;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.TrolleyOptionBay3;
                }

                return false;
            }
        }

        public bool BayRobotOption
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.RobotOptionBay1;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.RobotOptionBay2;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.RobotOptionBay3;
                }

                return false;
            }
        }

        public bool BayZeroChainIsVisible
        {
            get => this.bayZeroChainIsVisible;
            set => this.SetProperty(ref this.bayZeroChainIsVisible, value);
        }

        public bool IsExtraVertical => this.sensors.ElevatorOverrun;

        public bool IsHorizontalInconsistentBothHigh => (this.IsZeroChain && this.IsLoadingUnitOnElevator);

        public bool IsHorizontalInconsistentBothLow => (!this.IsZeroChain && !this.IsLoadingUnitOnElevator);

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

        public bool IsOneTonMachine => this.bayManagerService.Identity?.IsOneTonMachine ?? false;

        public bool IsZeroChain => this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneTon : this.sensors.ZeroPawlSensor;

        public bool IsZeroVertical => this.sensors.ZeroVerticalSensor;

        public bool ProfileCalibrationBay
        {
            get
            {
                if (this.Bay is null)
                {
                    return false;
                }

                if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
                {
                    return this.Sensors.ProfileCalibrationBay1;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
                {
                    return this.Sensors.ProfileCalibrationBay2;
                }
                else if (this.BayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
                {
                    return this.Sensors.ProfileCalibrationBay3;
                }

                return false;
            }
        }

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

        private bool IsHealthy => this.healthProbeService?.HealthMasStatus == HealthStatus.Healthy || this.healthProbeService?.HealthMasStatus == HealthStatus.Degraded;

        #endregion

        #region Methods

        public bool IsLoadingUnitInBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber)
        {
            if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                return this.Sensors.LUPresentInBay1;
            }
            else if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                return this.Sensors.LUPresentInBay2;
            }
            else if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                return this.Sensors.LUPresentInBay3;
            }

            return false;
        }

        public bool IsLoadingUnitInMiddleBottomBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber)
        {
            if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                return this.Sensors.LUPresentMiddleBottomBay1;
            }
            else if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                return this.Sensors.LUPresentMiddleBottomBay2;
            }
            else if (bayNumber is MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                return this.Sensors.LUPresentMiddleBottomBay3;
            }

            return false;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public async Task RefreshAsync(bool forceRefresh)
        {
            try
            {
                await this.GetBayAsync()
                    .ContinueWith(async (m) => await this.InitializeSensorsAsync());
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
                    }

                    if (this.Bay.Positions?.LastOrDefault() is BayPosition bayPositionUp)
                    {
                        this.BayPositionUpHeight = bayPositionUp.Height;
                    }

                    this.BayIsMultiPosition = this.Bay.IsDouble;

                    this.BayZeroChainIsVisible = this.Bay.IsExternal || this.Bay.Carousel != null;

                    this.shutterSensors.HasShutter = this.Bay.Shutter != null && this.Bay.Shutter.Type != ShutterType.NotSpecified;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task InitializeSensorsAsync()
        {
            var sensorsStates = await this.machineSensorsWebService.GetAsync();

            this.sensors.Update(sensorsStates.ToArray());
            if (this.Bay != null)
            {
                this.shutterSensors.Update(sensorsStates.ToArray(), (int)this.Bay.Number);
            }

            this.RaisePropertyChanged();
        }

        private async void OnHealthStatusChanged(HealthStatusChangedEventArgs status)
        {
            if (status.HealthMasStatus == HealthStatus.Healthy || status.HealthMasStatus == HealthStatus.Degraded)
            {
                await this.RefreshAsync(true);
            }
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (message?.Data?.SensorsStates != null)
            {
                this.sensors.Update(message.Data.SensorsStates);
                if (this.Bay == null)
                {
                    await this.GetBayAsync().ContinueWith((m) =>
                    {
                        if (this.IsHealthy)
                        {
                            this.shutterSensors.Update(message.Data.SensorsStates, (int)this.Bay.Number);
                        }
                    });
                }
                else
                {
                    this.shutterSensors.Update(message.Data.SensorsStates, (int)this.Bay.Number);
                }

                this.OnUpdateSensors?.Invoke(this, EventArgs.Empty);
            }

            this.RaisePropertyChanged();
        }

        private void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));
            this.RaisePropertyChanged(nameof(this.IsZeroChain));
            this.RaisePropertyChanged(nameof(this.IsExtraVertical));
            this.RaisePropertyChanged(nameof(this.IsZeroVertical));
            this.RaisePropertyChanged(nameof(this.BayZeroChain));
            this.RaisePropertyChanged(nameof(this.BayZeroChainIsVisible));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInMiddleBottomBay));
            this.RaisePropertyChanged(nameof(this.BayTrolleyOption));
            this.RaisePropertyChanged(nameof(this.BayRobotOption));
            this.RaisePropertyChanged(nameof(this.ProfileCalibrationBay));
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
            this.healthProbeToken = this.healthProbeService.HealthStatusChanged.Subscribe(this.OnHealthStatusChanged, ThreadOption.UIThread, false);

            this.sensorsToken = this.sensorsToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        async m => await this.OnSensorsChangedAsync(m),
                        ThreadOption.UIThread,
                        false,
                        m => m?.Data != null);

            //this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
            //    ??
            //    this.eventAggregator
            //        .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
            //        .Subscribe(
            //            this.OnBayChainPositionChanged,
            //            ThreadOption.UIThread,
            //            false);

            //this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
            //    ??
            //    this.eventAggregator
            //        .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
            //        .Subscribe(
            //            this.OnElevatorPositionChanged,
            //            ThreadOption.UIThread,
            //            false);

            //this.positioningOperationChangedToken = this.positioningOperationChangedToken
            //    ??
            //    this.eventAggregator
            //        .GetEvent<NotificationEventUI<PositioningMessageData>>()
            //        .Subscribe(
            //            async m => await this.OnPositioningOperationChangedAsync(m),
            //            ThreadOption.UIThread,
            //            false);
        }

        #endregion
    }
}
