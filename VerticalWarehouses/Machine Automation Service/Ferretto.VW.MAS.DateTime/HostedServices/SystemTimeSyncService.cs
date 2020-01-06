using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement.Models;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class SystemTimeSyncService : BackgroundService, ISystemTimeSyncService
    {
        #region Fields

        private const int SyncToleranceMilliseconds = 5000;

        private readonly PubSubEvent<SystemTimeChangedEventArgs> timeChangedEvent;

        private readonly IUtcTimeWmsWebService utcTimeWmsWebService;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructors

        public SystemTimeSyncService(
            IEventAggregator eventAggregator,
            IWmsSettingsProvider wmsSettingsProvider,
            IUtcTimeWmsWebService utcTimeWmsWebService)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();

            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.utcTimeWmsWebService = utcTimeWmsWebService ?? throw new ArgumentNullException(nameof(utcTimeWmsWebService));
        }

        #endregion

        #region Methods

        public void Disable()
        {
            this.cancellationTokenSource?.Cancel();
        }

        public void Enable()
        {
            this.Disable();

            Task.Run(this.ExecutePollingAsync);
        }

        public void SetSystemTime(DateTime dateTime)
        {
            dateTime.SetAsSystemTime();

            this.timeChangedEvent.Publish(new SystemTimeChangedEventArgs(dateTime));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            this.cancellationTokenSource?.Cancel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.wmsSettingsProvider.IsWmsTimeSyncEnabled)
            {
                this.Enable();
            }

            return Task.CompletedTask;
        }

        private async Task ExecutePollingAsync()
        {
            var syncIntervalMilliseconds = this.wmsSettingsProvider.TimeSyncIntervalMilliseconds;

            this.cancellationTokenSource = new CancellationTokenSource();

            try
            {
                do
                {
                    try
                    {
                        var machineUtcTimeBefore = DateTimeOffset.UtcNow;
                        var remoteUtcTime = await this.utcTimeWmsWebService.GetAsync(this.cancellationTokenSource.Token);

                        if (Math.Abs((machineUtcTimeBefore - remoteUtcTime).TotalMilliseconds) > SyncToleranceMilliseconds)
                        {
                            System.Diagnostics.Debug.Assert(remoteUtcTime.Kind is DateTimeKind.Utc);

                            var newMachineLocalTime = remoteUtcTime.ToLocalTime();
                            this.SetSystemTime(newMachineLocalTime);
                        }
                    }
                    catch (WmsWebApiException)
                    {
                    }

                    await Task.Delay(syncIntervalMilliseconds, this.cancellationTokenSource.Token);
                }
                while (!this.cancellationTokenSource.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        #endregion
    }
}
