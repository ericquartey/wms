using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineModeService : IMachineModeService, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly SubscriptionToken sensorsToken;

        private bool isDisposed;

        private bool? runningState;

        #endregion

        #region Constructors

        public MachineModeService(
            IEventAggregator eventAggregator,
            IMachinePowerWebService machinePowerWebService,
            IMachineModeWebService machineModeWebService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machinePowerWebService = machinePowerWebService ?? throw new ArgumentNullException(nameof(machinePowerWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));

            this.machinePowerChangedToken = this.eventAggregator
              .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
              .Subscribe(
                  this.OnMachinePowerChanged,
                  ThreadOption.UIThread,
                  false);

            this.machineModeChangedToken = this.eventAggregator
               .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
               .Subscribe(
                   this.OnMachineModeChanged,
                   ThreadOption.UIThread,
                   false);

            this.healthStatusChangedToken = this.eventAggregator
                .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.sensorsToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        async (m) => await this.OnSensorsChangedAsync(m),
                        ThreadOption.UIThread,
                        false,
                        (m) =>
                        {
                            var res = !this.runningState.HasValue ||
                                      (m.Data.SensorsStates[(int)IOMachineSensors.RunningState] != this.runningState.Value);
                            this.runningState = m.Data.SensorsStates[(int)IOMachineSensors.RunningState];
                            return res;
                        });

            this.GetMachineStatusAsync().ConfigureAwait(false);
        }

        #endregion

        #region Properties

        public MachineMode MachineMode { get; private set; }

        public MachinePowerState MachinePower { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public async Task GetMachineStatusAsync()
        {
            try
            {
                this.MachinePower = await this.machinePowerWebService.GetAsync();
                this.MachineMode = await this.machineModeWebService.GetAsync();

                this.eventAggregator
                    .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                    .Publish(new MachineModeChangedEventArgs(this.MachineMode));

                this.eventAggregator
                    .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                    .Publish(new MachinePowerChangedEventArgs(this.MachinePower));
            }
            catch
            {
                // do nothing
            }
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

        public async Task SetAutomaticMode()
        {
            try
            {
                await this.machineModeWebService.SetAutomaticAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        public async Task SetManualMode()
        {
            try
            {
                await this.machineModeWebService.SetManualAsync();
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
                this.machineModeChangedToken.Dispose();
                this.machinePowerChangedToken.Dispose();
                this.healthStatusChangedToken.Dispose();
            }

            this.isDisposed = true;
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Healthy
                ||
                e.HealthStatus == HealthStatus.Degraded)
            {
                await this.GetMachineStatusAsync();
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.logger.Debug($"Machine mode changed to '{e.MachineMode}'.");

            this.MachineMode = e.MachineMode;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.logger.Debug($"Machine power state changed to '{e.MachinePowerState}'.");

            this.MachinePower = e.MachinePowerState;
        }

        private async Task OnSensorsChangedAsync(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            await this.GetMachineStatusAsync();
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
