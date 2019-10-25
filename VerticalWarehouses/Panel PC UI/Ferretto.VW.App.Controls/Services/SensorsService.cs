using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
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

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private bool bayIsMultiPosition;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private bool isShutterTwoSensors;

        private bool isZeroChain;

        private SubscriptionToken positioningToken;

        private SubscriptionToken sensorsToken;

        private ShutterSensors shutterSensors;

        #endregion

        #region Constructors

        public SensorsService(
         IMachineElevatorWebService machineElevatorWebService,
         IMachineSensorsWebService machineSensorsWebService,
         IEventAggregator eventAggregator,
         IBayManager bayManagerService)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));

            this.StartMonitoring();
        }

        #endregion

        #region Properties

        public Bay Bay { get; private set; }

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
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

        public bool IsZeroChain
        {
            get => this.isZeroChain;
            set => this.SetProperty(ref this.isZeroChain, value);
        }

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

        #endregion

        #region Methods

        public void EndMonitoring()
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public async Task RefreshAsync()
        {
            this.IsZeroChain = this.IsOneTonMachine
                ? this.sensors.ZeroPawlSensorOneK
                : this.sensors.ZeroPawlSensor;

            await this.RetrieveElevatorPositionAsync();

            this.Bay = await this.bayManagerService.GetBayAsync();

            this.BayIsMultiPosition = this.Bay.IsDouble;

            this.IsShutterTwoSensors = this.Bay.Shutter.Type == MAS.AutomationService.Contracts.ShutterType.TwoSensors;

            this.shutterSensors = new ShutterSensors((int)this.Bay.Number);

            await this.InitializeSensors();
        }

        public void StartMonitoring()
        {
            this.sensorsToken = this.sensorsToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
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
        }

        private async Task InitializeSensors()
        {
            try
            {
                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
                this.ShutterSensors.Update(sensorsStates.ToArray());
            }
            catch
            {
            }
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

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
            this.ShutterSensors.Update(message.Data.SensorsStates);

            this.IsZeroChain = this.IsOneTonMachine
                ? this.sensors.ZeroPawlSensorOneK
                : this.sensors.ZeroPawlSensor;

            this.RaisePropertyChanged(nameof(this.IsLoadingUnitOnElevator));
            this.RaisePropertyChanged(nameof(this.IsLoadingUnitInBay));
        }

        private async Task RetrieveElevatorPositionAsync()
        {
            try
            {
                this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
            }
            catch
            {
            }
        }

        #endregion
    }
}
