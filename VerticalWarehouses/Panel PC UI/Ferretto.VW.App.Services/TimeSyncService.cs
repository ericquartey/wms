using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class TimeSyncService : ITimeSyncService, IDisposable
    {
        #region Fields

        private const int DefaultSyncInterval = 60 * 60 * 1000;

        private const int MinimumSyncInterval = 10 * 1000;

        private const int SyncToleranceMilliseconds = 10 * 1000;

        private readonly Logger logger;

        private readonly PubSubEvent<SystemTimeChangedEventArgs> systemTimeChangedEvent;

        private readonly IMachineUtcTimeWebService utcTimeWebService;

        private bool isDisposed;

        private int syncIntervalMilliseconds = DefaultSyncInterval;

        private SubscriptionToken systemTimeToken;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public TimeSyncService(
            IMachineUtcTimeWebService utcTimeWebService,
            IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.utcTimeWebService = utcTimeWebService ?? throw new ArgumentNullException(nameof(utcTimeWebService));
            this.systemTimeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
            this.logger = NLog.LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Properties

        public int SyncIntervalMilliseconds
        {
            get => this.syncIntervalMilliseconds;
            set => this.syncIntervalMilliseconds = Math.Max(MinimumSyncInterval, value);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.Stop();

            this.isDisposed = true;
        }

        public void Start()
        {
            this.systemTimeToken = this.systemTimeToken
                ??
                this.systemTimeChangedEvent.Subscribe(
                    async e => await this.OnSystemTimeChangedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.tokenSource?.Cancel();
            this.tokenSource = new CancellationTokenSource();
            this.logger.Debug("Starting Time sync service.");

            Task.Run(async () =>
            {
                var cancellationToken = this.tokenSource.Token;

                try
                {
                    do
                    {
                        await this.SyncTimeWithAutomationServiceAsync();

                        await Task.Delay(this.SyncIntervalMilliseconds, cancellationToken);
                    }
                    while (!cancellationToken.IsCancellationRequested);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
        }

        public void Stop()
        {
            this.logger.Debug("Stopping Time sync service.");
            this.tokenSource?.Cancel();
            this.tokenSource = null;

            this.systemTimeToken?.Dispose();
            this.systemTimeToken = null;
        }

        private async Task OnSystemTimeChangedAsync(SystemTimeChangedEventArgs e)
        {
            var remoteUtcTime = e.DateTime;
            var machineUtcTime = DateTimeOffset.UtcNow;

            if (Math.Abs((machineUtcTime - remoteUtcTime).TotalMilliseconds) > SyncToleranceMilliseconds)
            {
                remoteUtcTime.SetAsUtcSystemTime();
                this.logger.Info($"PPC time was synced with remote time. Machine {machineUtcTime} Remote {remoteUtcTime}");
            }
        }

        private async Task SyncTimeWithAutomationServiceAsync()
        {
            try
            {
                this.logger.Trace("Attempting to sync PPC time with MAS time.");

                var remoteUtcTime = await this.utcTimeWebService.GetAsync();
                var machineUtcTime = DateTimeOffset.UtcNow;

                if (Math.Abs((machineUtcTime - remoteUtcTime).TotalMilliseconds) > SyncToleranceMilliseconds)
                {
                    remoteUtcTime.SetAsUtcSystemTime();
                    this.logger.Info($"PPC time was synced with MAS time. Machine {machineUtcTime} Remote {remoteUtcTime}");
                }
                else
                {
                    this.logger.Trace($"PPC is alredy synced with MAS time. Machine {machineUtcTime} Remote {remoteUtcTime}");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Cannot sync time with MAS: '{ex.Message}'.");
            }
        }

        #endregion
    }
}
