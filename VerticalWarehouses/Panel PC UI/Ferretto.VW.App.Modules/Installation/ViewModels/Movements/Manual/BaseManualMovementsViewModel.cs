using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal abstract class BaseManualMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly Sensors sensors = new Sensors();

        private readonly ShutterSensors shutterSensors = new ShutterSensors();

        private Bay bay;

        private int bayNumber;

        private bool isStopping;

        private SubscriptionToken movementsSubscriptionToken;

        private SubscriptionToken sensorsToken;

        private DelegateCommand stopMovementCommand;

        private bool unsafeRelease;

        #endregion

        #region Constructors

        protected BaseManualMovementsViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineSensorsWebService machineSensorsWebService,
            IHealthProbeService healthProbeService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.MachineElevatorService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            protected set => this.SetProperty(ref this.bayNumber, value);
        }

        public bool IsOneTonMachine => this.bayManagerService.Identity.IsOneTonMachine;

        public bool IsStopping
        {
            get => this.isStopping;
            protected set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public override bool IsWaitingForResponse
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

        public bool IsZeroChain => this.IsOneTonMachine ? this.sensors.ZeroPawlSensorOneK : this.sensors.ZeroPawlSensor;

        public Sensors Sensors => this.sensors;

        public ShutterSensors ShutterSensors => this.shutterSensors;

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync(), this.CanExecuteStopMovment));

        public bool UnsafeRelease
        {
            get => this.unsafeRelease;
            set => this.SetProperty(ref this.unsafeRelease, value, this.RaiseCanExecuteChanged);
        }

        protected IMachineElevatorWebService MachineElevatorService { get; }

        #endregion

        #region Methods

        public void DisableAllExceptThis()
        {
            var name = this.GetType().ToString();
            this.EventAggregator
                .GetEvent<ManualMovementsChangedPubSubEvent>()
                .Publish(new ManualMovementsChangedMessage(name));
        }

        public override void Disappear()
        {
            base.Disappear();

            this.movementsSubscriptionToken?.Dispose();
            this.movementsSubscriptionToken = null;

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
        }

        public void EnableAll()
        {
            this.EventAggregator
               .GetEvent<ManualMovementsChangedPubSubEvent>()
               .Publish(new ManualMovementsChangedMessage(null));
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.RetrieveCurrentPositionAsync()
                .ContinueWith(async (w) =>
                {
                    if (!w.IsFaulted)
                    {
                        await this.InitializeSensors();
                    }
                });

            await base.OnAppearedAsync();

            this.EnableAll();

            this.RaiseCanExecuteChanged();
        }

        protected virtual bool CanExecuteStopMovment()
        {
            return true;
        }

        protected virtual void EnabledChanged(ManualMovementsChangedMessage message)
        {
            if (string.IsNullOrEmpty(message.ViewModelName))
            {
                this.IsEnabled = true;
                return;
            }

            var name = this.GetType().ToString();
            if (!name.Equals(message.ViewModelName))
            {
                this.IsEnabled = false;
            }
        }

        protected abstract void OnErrorStatusChanged();

        protected override async Task OnErrorStatusChangedAsync(MachineErrorEventArgs e)
        {
            await base.OnErrorStatusChangedAsync(e);

            this.OnErrorStatusChanged();
        }

        protected abstract void OnMachinePowerChanged();

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
            }

            this.OnMachinePowerChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Sensors));
            this.RaisePropertyChanged(nameof(this.ShutterSensors));
        }

        protected abstract Task StopMovementAsync();

        private async Task InitializeSensors()
        {
            if (this.healthProbeService.HealthStatus == HealthStatus.Healthy)
            {
                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());

                this.shutterSensors.Update(sensorsStates.ToArray(), (int)this.bay.Number);

                this.RaisePropertyChanged();
            }
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates.ToArray());

            this.shutterSensors.Update(message.Data.SensorsStates.ToArray(), (int)this.bay.Number);

            this.RaiseCanExecuteChanged();
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.bay = await this.bayManagerService.GetBayAsync();
                this.BayNumber = (int)this.bay.Number;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void SubscribeToEvents()
        {
            this.movementsSubscriptionToken = this.movementsSubscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<ManualMovementsChangedPubSubEvent>()
                    .Subscribe(
                        this.EnabledChanged,
                        ThreadOption.UIThread,
                        false,
                        message => message != null);

            this.sensorsToken = this.sensorsToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);
        }

        #endregion
    }
}
