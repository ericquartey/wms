using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.TimeManagement.Models;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class SystemTimeSyncService : BackgroundService
    {
        #region Fields

        private const int MinResyncPeriodMilliseconds = 60 * 1000;

        private const int SyncToleranceMilliseconds = 10 * 1000;

        private readonly IDataLayerService dataLayerService;

        private readonly ILogger<SystemTimeSyncService> logger;

        private readonly NotificationEvent notificationEvent;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly PubSubEvent<SyncStateChangeRequestEventArgs> syncStateChangeRequestEvent;

        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructors

        public SystemTimeSyncService(
            IEventAggregator eventAggregator,
            IDataLayerService dataLayerService,
            ILogger<SystemTimeSyncService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.syncStateChangeRequestEvent = eventAggregator.GetEvent<PubSubEvent<SyncStateChangeRequestEventArgs>>();
        }

        #endregion

        #region Methods

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

            this.notificationEvent.Subscribe(
                   this.OnDataLayerReady,
                   ThreadOption.PublisherThread,
                   false,
                   m => m.Type is CommonUtils.Messages.Enumerations.MessageType.WmsEnableChanged);

            this.syncStateChangeRequestEvent.Subscribe(
                  this.OnSyncStateChangeRequested,
                  ThreadOption.PublisherThread,
                  false);
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

        private void Disable()
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource = null;
        }

        private void Enable()
        {
            this.Disable();

            Task.Run(this.ExecutePollingAsync);
        }

        private void ExecuteBackupScript()
        {
            var backupScript = "f:\\database\\remote_backup.cmd";
            var info = new FileInfo(backupScript);
            if (info.Exists)
            {
                var script = File.ReadAllText(backupScript);
                if (!string.IsNullOrEmpty(script))
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/C {script}";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.ErrorDialog = false;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    try
                    {
                        process.Start();
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex.Message);
                    }
                    finally
                    {
                        process.Close();
                    }
                }
            }
        }

        private async Task ExecutePollingAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = this.cancellationTokenSource.Token;

            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var wmsSettingsProvider = scope.ServiceProvider.GetRequiredService<IWmsSettingsProvider>();
                    var machineVolatileDataProvider = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();

                    do
                    {
                        var syncIntervalMilliseconds = wmsSettingsProvider.TimeSyncIntervalMilliseconds;
                        try
                        {
                            if (!machineVolatileDataProvider.IsDeviceManagerBusy)
                            {
                                this.logger.LogDebug("Attempting sync time with WMS.");
                                var utcTimeWmsWebService = scope.ServiceProvider.GetRequiredService<IUtcTimeWmsWebService>();
                                var remoteUtcTime = await utcTimeWmsWebService.GetAsync(cancellationToken);
                                var machineUtcTime = DateTimeOffset.Now;

                                var timeDifference = machineUtcTime - remoteUtcTime;

                                this.logger.LogTrace("Machine (UTC offset): '{time}'.", machineUtcTime);
                                this.logger.LogTrace("Remote  (UTC offset): '{time}' .", remoteUtcTime);
                                this.logger.LogTrace("Time difference: '{timespan}'.", machineUtcTime - remoteUtcTime);

                                this.logger.LogTrace("Machine (local time): '{time}'.", machineUtcTime.LocalDateTime);
                                this.logger.LogTrace("Remote  (local time): '{time}' .", remoteUtcTime.LocalDateTime);

                                var timeDifferenceMilliseconds = Math.Abs(timeDifference.TotalMilliseconds);
                                if (timeDifferenceMilliseconds > SyncToleranceMilliseconds)
                                {
                                    var systemTimeProvider = scope.ServiceProvider.GetRequiredService<IInternalSystemTimeProvider>();
                                    systemTimeProvider.SetUtcTime(remoteUtcTime.UtcDateTime);

                                    this.logger.LogDebug("Time synced successfully. from time '{machine}' to time '{remote}'", machineUtcTime.LocalDateTime, remoteUtcTime.LocalDateTime);
                                }

                                wmsSettingsProvider.LastWmsSyncTime = DateTimeOffset.UtcNow;

                                this.ExecuteBackupScript();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogWarning("Unable to sync machine time with WMS: '{details}'.", ex.Message);
                        }

                        if ((DateTimeOffset.UtcNow - wmsSettingsProvider.LastWmsSyncTime).TotalMilliseconds > syncIntervalMilliseconds)
                        {
                            this.logger.LogTrace("It's been too long since last WMS time sync. Will attempt resync in {timespan}.", TimeSpan.FromMilliseconds(SyncToleranceMilliseconds));

                            await Task.Delay(SyncToleranceMilliseconds, cancellationToken);
                        }
                        else
                        {
                            this.logger.LogTrace("Sleeping until next time sync (scheduled in {timespan}).", TimeSpan.FromMilliseconds(syncIntervalMilliseconds));
                            await Task.Delay(syncIntervalMilliseconds, cancellationToken);
                        }
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                this.logger.LogTrace("Stopping service.");
                return;
            }
        }

        private void OnDataLayerReady(NotificationMessage message)
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var wmsSettingsProvider = scope.ServiceProvider.GetRequiredService<IWmsSettingsProvider>();

                if (wmsSettingsProvider.IsTimeSyncEnabled && wmsSettingsProvider.IsEnabled)
                {
                    this.logger.LogDebug("Starting WMS time sync service.");

                    this.Enable();
                }
                else
                {
                    this.logger.LogDebug("Stopping WMS time sync service.");

                    this.Disable();
                }
            }
        }

        private void OnSyncStateChangeRequested(SyncStateChangeRequestEventArgs e)
        {
            if (e.Enable)
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
        }

        #endregion
    }
}
