using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public sealed class MachineModeService : IMachineModeService, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private SubscriptionToken healthSubscriptionToken;

        private bool isDisposed;

        private MachinePowerState machinePower;

        private SubscriptionToken sensorsSubscriptionToken;

        #endregion

        #region Constructors

        public MachineModeService(
            IEventAggregator eventAggregator,
            IHealthProbeService healthProbeService,
            IMachineSensorsWebService machineSensorsWebService,
            IMachinePowerWebService machinePowerWebService)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (healthProbeService is null)
            {
                throw new ArgumentNullException(nameof(healthProbeService));
            }

            if (machineSensorsWebService is null)
            {
                throw new ArgumentNullException(nameof(machineSensorsWebService));
            }

            if (machinePowerWebService is null)
            {
                throw new ArgumentNullException(nameof(machinePowerWebService));
            }

            this.eventAggregator = eventAggregator;
            this.healthProbeService = healthProbeService;
            this.machineSensorsWebService = machineSensorsWebService;
            this.machinePowerWebService = machinePowerWebService;

            this.sensorsSubscriptionToken = this.eventAggregator
               .GetEvent<NotificationEventUI<ChangeRunningStateMessageData>>()
               .Subscribe(
                   message => this.OnRunningStateChanged(message),
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
                await this.machinePowerWebService.PowerOffAsync();
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
                await this.machinePowerWebService.PowerOnAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.sensorsSubscriptionToken?.Dispose();
                this.sensorsSubscriptionToken = null;

                this.healthSubscriptionToken?.Dispose();
                this.healthSubscriptionToken = null;
            }

            this.isDisposed = true;
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Healthy
                ||
                e.HealthStatus == HealthStatus.Degraded)
            {
                try
                {
                    var isPoweredOn = await this.machinePowerWebService.IsPoweredOnAsync();

                    this.MachinePower = isPoweredOn ? MachinePowerState.Powered : MachinePowerState.Unpowered;
                }
                catch (Exception ex)
                {
                    this.ShowError(ex);
                }
            }
        }

        private void OnRunningStateChanged(NotificationMessageUI<ChangeRunningStateMessageData> message)
        {
            var runningState = message.Status == MessageStatus.OperationEnd && message.Data.Enable;
            this.MachinePower = runningState ? MachinePowerState.Powered : MachinePowerState.Unpowered;
        }

        private void ShowError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        #endregion
    }
}
