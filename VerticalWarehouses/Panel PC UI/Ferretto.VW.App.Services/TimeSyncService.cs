using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using NLog;

namespace Ferretto.VW.App.Services
{
    internal sealed class TimeSyncService : ITimeSyncService, IDisposable
    {
        #region Fields

        private const int DefaultSyncInterval = 60 * 60 * 1000;

        private const int MinimumSyncInterval = 10 * 1000;

        private const int SyncToleranceMilliseconds = 10 * 1000;

        private readonly Logger logger;

        private readonly IMachineUtcTimeWebService utcTimeWebService;

        private bool isDisposed;

        private int syncIntervalMilliseconds = DefaultSyncInterval;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public TimeSyncService(IMachineUtcTimeWebService utcTimeWebService)
        {
            this.utcTimeWebService = utcTimeWebService ?? throw new ArgumentNullException(nameof(utcTimeWebService));
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

            this.tokenSource?.Dispose();
            this.tokenSource = null;

            this.isDisposed = true;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                this.tokenSource?.Cancel();
                this.tokenSource = new CancellationTokenSource();

                try
                {
                    do
                    {
                        try
                        {
                            this.logger.Trace("Attempting to sync PPC time with MAS time.");

                            var remoteUtcTime = await this.utcTimeWebService.GetAsync();
                            var machineUtcTime = DateTimeOffset.UtcNow;

                            if ((machineUtcTime - remoteUtcTime).TotalSeconds > SyncToleranceMilliseconds)
                            {
                                remoteUtcTime.LocalDateTime.SetAsUtcSystemTime();
                                this.logger.Trace("PPC time was synced with MAS time.");
                            }
                            else
                            {
                                this.logger.Trace("PPC is alredy synced with MAS time.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error($"Cannot sync time with MAS: '{ex.Message}'.");
                        }

                        await Task.Delay(this.SyncIntervalMilliseconds, this.tokenSource.Token);
                    }
                    while (!this.tokenSource.IsCancellationRequested);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
        }

        public void Stop()
        {
            this.tokenSource?.Cancel();
        }

        #endregion
    }
}
