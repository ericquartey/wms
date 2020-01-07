using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.TimeManagement.Models;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class SystemTimeSyncService : BackgroundService, ISystemTimeSyncService
    {
        #region Fields

        private const int MinResyncPeriodMilliseconds = 10000;

        private const int SyncToleranceMilliseconds = 5000;

        private readonly IDataLayerService dataLayerService;

        private readonly ILogger<SystemTimeSyncService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly PubSubEvent<SystemTimeChangedEventArgs> timeChangedEvent;

        private readonly IUtcTimeWmsWebService utcTimeWmsWebService;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructors

        public SystemTimeSyncService(
            IEventAggregator eventAggregator,
            IDataLayerService dataLayerService,
            IWmsSettingsProvider wmsSettingsProvider,
            IUtcTimeWmsWebService utcTimeWmsWebService,
            ILogger<SystemTimeSyncService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.utcTimeWmsWebService = utcTimeWmsWebService ?? throw new ArgumentNullException(nameof(utcTimeWmsWebService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(utcTimeWmsWebService));
            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
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
#if !DEBUG
            dateTime.SetAsSystemTime();
#endif

            this.timeChangedEvent.Publish(new SystemTimeChangedEventArgs(dateTime));
        }

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.dataLayerService.IsReady)
            {
                this.OnDataLayerReady(null);
            }
            else
            {
                this.notificationEvent.Subscribe(
                    this.OnDataLayerReady,
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            this.cancellationTokenSource?.Cancel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        private async Task ExecutePollingAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = this.cancellationTokenSource.Token;

            try
            {
                var syncIntervalMilliseconds = this.wmsSettingsProvider.TimeSyncIntervalMilliseconds;

                do
                {
                    try
                    {
                        this.logger.LogTrace("Attempting sync time with WMS.");
                        var remoteUtcTime = await this.utcTimeWmsWebService.GetAsync(cancellationToken);
                        var machineUtcTime = DateTimeOffset.UtcNow;

                        var timeDifferenceMilliseconds = Math.Abs((machineUtcTime - remoteUtcTime).TotalMilliseconds);
                        if (timeDifferenceMilliseconds > SyncToleranceMilliseconds)
                        {
                            System.Diagnostics.Debug.Assert(remoteUtcTime.Kind is DateTimeKind.Utc);

                            var newMachineLocalTime = remoteUtcTime.ToLocalTime();
                            this.SetSystemTime(newMachineLocalTime);
                            this.wmsSettingsProvider.LastWmsSyncTime = DateTimeOffset.UtcNow;
                            this.logger.LogTrace("Time synced successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning("Unable to sync machine time with WMS: '{details}'.", ex.Message);
                    }

                    if ((DateTimeOffset.UtcNow - this.wmsSettingsProvider.LastWmsSyncTime).TotalMilliseconds > syncIntervalMilliseconds)
                    {
                        this.logger.LogTrace("It's been too long since last WMS time sync. Will attempt resync in {timespan}.", TimeSpan.FromMilliseconds(MinResyncPeriodMilliseconds));

                        await Task.Delay(MinResyncPeriodMilliseconds, cancellationToken);
                    }
                    else
                    {
                        this.logger.LogTrace("Sleeping until next time sync (scheduled in {timespan}).", TimeSpan.FromMilliseconds(syncIntervalMilliseconds));
                        await Task.Delay(syncIntervalMilliseconds, cancellationToken);
                    }
                }
                while (!cancellationToken.IsCancellationRequested);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                this.logger.LogTrace("Stopping service.");
                return;
            }
        }

        private void OnDataLayerReady(NotificationMessage message)
        {
            if (this.wmsSettingsProvider.IsWmsTimeSyncEnabled)
            {
                this.logger.LogTrace("Data layer is ready, starting WMS time sync service.");

                this.Enable();
            }
        }

        #endregion
    }
}
