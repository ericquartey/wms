using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public sealed class MachineModeService : IMachineModeService, IDisposable
    {

        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly MAS.AutomationService.Contracts.IMachineSensorsService machineSensorsService;

        private readonly MAS.AutomationService.Contracts.IMachineMachineStatusService machineStatusService;

        private bool disposedValue;

        private SubscriptionToken healthSubscriptionToken;

        private MachinePowerState machinePower;

        private SubscriptionToken sensorsSubscriptionToken;

        #endregion

        #region Constructors

        public MachineModeService(
            IEventAggregator eventAggregator,
            IHealthProbeService healthProbeService,
            MAS.AutomationService.Contracts.IMachineSensorsService machineSensorsService,
            MAS.AutomationService.Contracts.IMachineMachineStatusService machineStatusService)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (healthProbeService is null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            if (machineSensorsService is null)
            {
                throw new ArgumentNullException(nameof(machineSensorsService));
            }

            if (machineStatusService is null)
            {
                throw new ArgumentNullException(nameof(machineStatusService));
            }

            this.eventAggregator = eventAggregator;
            this.healthProbeService = healthProbeService;
            this.machineSensorsService = machineSensorsService;
            this.machineStatusService = machineStatusService;

            this.sensorsSubscriptionToken = this.eventAggregator
               .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
               .Subscribe(
                   message => this.OnSensorsChanged(message?.Data?.SensorsStates),
                   ThreadOption.UIThread,
                   false);

            this.healthSubscriptionToken = this.healthProbeService.HealthStatusChanged
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedAsync(e),
                    ThreadOption.UIThread,
                    false);
        }

        #endregion



        #region Properties

        public MachineMode MachineMode { get; }

        public MachineModeChangedPubSubEvent MachineModeChangedEvent => this.eventAggregator.GetEvent<MachineModeChangedPubSubEvent>();

        public MachinePowerState MachinePower
        {
            get => this.machinePower;
            set
            {
                if (this.machinePower != value)
                {
                    this.machinePower = value;

                    this.MachineModeChangedEvent
                        .Publish(new MachineModeChangedEventArgs(this.MachineMode, this.MachinePower));
                }
            }
        }

        #endregion



        #region Methods

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public async Task PowerOffAsync()
        {
            try
            {
                await this.machineStatusService.PowerOffAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        public async Task PowerOnAsync()
        {
            try
            {
                await this.machineStatusService.PowerOnAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.sensorsSubscriptionToken == null)
                    {
                        this.eventAggregator
                            .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                            .Unsubscribe(this.sensorsSubscriptionToken);

                        this.sensorsSubscriptionToken = null;
                    }

                    if (this.healthSubscriptionToken == null)
                    {
                        this.eventAggregator
                            .GetEvent<HealthStatusChangedPubSubEvent>()
                            .Unsubscribe(this.healthSubscriptionToken);

                        this.healthSubscriptionToken = null;
                    }
                }

                this.disposedValue = true;
            }
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Healthy
                ||
                e.HealthStatus == HealthStatus.Degraded)
            {
                try
                {
                    var sensorStates = await this.machineSensorsService.GetAsync();

                    this.OnSensorsChanged(sensorStates.ToArray());
                }
                catch (Exception ex)
                {
                    this.ShowError(ex);
                }
            }
        }

        private void OnSensorsChanged(bool[] sensorsStates)
        {
            if (sensorsStates is null)
            {
                this.logger.Warn("Unable to update machine power state: empty sensors state array received.");
                return;
            }

            var sensorIndex = (int)IOMachineSensors.RunningState;

            if (sensorsStates.Length > sensorIndex)
            {
                var isPoweredOn = sensorsStates[sensorIndex];

                this.MachinePower = isPoweredOn ? MachinePowerState.Powered : MachinePowerState.Unpowered;
            }
            else
            {
                this.MachinePower = MachinePowerState.Unknown;

                this.logger.Warn("Unable to update machine power state: sensors state array length was shorter than expected.");
            }
        }

        private void ShowError(Exception ex)
        {
            this.eventAggregator
             .GetEvent<PresentationChangedPubSubEvent>()
             .Publish(new PresentationChangedMessage(ex));
        }

        #endregion
    }
}
