using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Services
{
    internal sealed class TimeSyncService : ITimeSyncService, IDisposable
    {
        #region Fields

        private const int DefaultSyncInterval = 60 * 60 * 1000;

        private const int MinimumSyncInterval = 10 * 1000;

        private const int SyncToleranceMilliseconds = 10 * 1000;

        private readonly IUtcTimeWmsWebService utcTimeWmsWebService;

        private bool isDisposed;

        private int syncIntervalMilliseconds = DefaultSyncInterval;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public TimeSyncService(IUtcTimeWmsWebService utcTimeWmsWebService)
        {
            this.utcTimeWmsWebService = utcTimeWmsWebService ?? throw new ArgumentNullException(nameof(utcTimeWmsWebService));
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
                            var machineUtcTimeBefore = DateTimeOffset.UtcNow;
                            var remoteUtcTime = await this.utcTimeWmsWebService.GetAsync();
                            var machineUtcTimeAfter = DateTimeOffset.UtcNow;

                            var callDuration = machineUtcTimeAfter - machineUtcTimeBefore;
                            var callAdjustmentSeconds = TimeSpan.FromSeconds(callDuration.TotalSeconds / 2.0);

                            var adjustedRemoteUtcTime = remoteUtcTime.Subtract(callAdjustmentSeconds);

                            if ((machineUtcTimeBefore - adjustedRemoteUtcTime).TotalSeconds > SyncToleranceMilliseconds)
                            {
                                adjustedRemoteUtcTime.SetAsSystemTime();
                            }
                        }
                        catch
                        {
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
