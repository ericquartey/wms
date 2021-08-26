using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineModeService : IMachineModeService, IDisposable
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly IMachineWmsStatusWebService machineWmsStatusWebService;

        private bool isDisposed;

        #endregion

        #region Constructors

        public MachineModeService(
            IEventAggregator eventAggregator,
            IMachinePowerWebService machinePowerWebService,
            IMachineModeWebService machineModeWebService,
            IMachineWmsStatusWebService machineWmsStatusWebService,
            IBayManager bayManager)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machinePowerWebService = machinePowerWebService ?? throw new ArgumentNullException(nameof(machinePowerWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.machineWmsStatusWebService = machineWmsStatusWebService ?? throw new ArgumentNullException(nameof(machineWmsStatusWebService));
            this.bayManager = bayManager;

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

            this.GetMachineStatusAsync().ConfigureAwait(false);
        }

        #endregion

        #region Properties

        public bool IsWmsEnabled { get; private set; }

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
                var machinePower = await this.machinePowerWebService.GetAsync();
                var machineMode = await this.machineModeWebService.GetAsync();

                this.IsWmsEnabled = await this.machineWmsStatusWebService.IsEnabledAsync();

                if (this.MachinePower != machinePower)
                {
                    this.MachinePower = machinePower;

                    this.eventAggregator
                        .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                        .Publish(new MachinePowerChangedEventArgs(this.MachinePower));
                }

                if (this.MachineMode != machineMode)
                {
                    this.MachineMode = machineMode;

                    this.eventAggregator
                        .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                        .Publish(new MachineModeChangedEventArgs(this.MachineMode));
                }
            }
            catch
            {
                // do nothing
            }
        }

        public async Task OnUpdateServiceAsync()
        {
            await this.GetMachineStatusAsync();
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
            if (e.HealthMasStatus == HealthStatus.Healthy
                ||
                e.HealthMasStatus == HealthStatus.Degraded)
            {
                await this.GetMachineStatusAsync();
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.logger.Debug($"Machine mode changed to '{e.MachineMode}'.");

            this.MachineMode = e.MachineMode;

            if (this.MachineMode == MachineMode.Shutdown && this.MachinePower == MachinePowerState.Unpowered)
            {
                this.logger.Debug($"Machine shutdown detected");
                this.Shutdown();
            }
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.logger.Debug($"Machine power state changed to '{e.MachinePowerState}'.");

            this.MachinePower = e.MachinePowerState;

            if (this.MachineMode == MachineMode.Shutdown && this.MachinePower == MachinePowerState.Unpowered)
            {
                this.logger.Debug($"Machine shutdown detected");
                this.Shutdown();
            }
        }

        private void ShowError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private void Shutdown()
        {
            this.logger.Warn("Close MAS service");
            var sc = new ServiceController();
            sc.ServiceName = ConfigurationManager.AppSettings.GetAutomationServiceName();
            try
            {
                sc.Stop();
            }
            catch (Exception ex)
            {
                this.logger.Debug(ex.ToString());
            }
            finally
            {
                sc.Dispose();
            }

            this.logger.Warn("Shutdown pc");
            var psi = new ProcessStartInfo("shutdown", "/r /t 4");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);

            this.logger.Warn("Close application");
            System.Windows.Application.Current.Shutdown();
        }

        #endregion
    }
}
