using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService
{
    internal sealed class DatabaseCleanupService : BackgroundService
    {
        #region Fields

        private static readonly TimeSpan DefaultExecutionTimespan = TimeSpan.FromHours(12);

        private static readonly TimeSpan DefaultMaximumLogTimespan = TimeSpan.FromDays(7);

        private readonly IConfiguration configuration;

        private readonly ILogger<DatabaseCleanupService> logger;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private TimeSpan executionTimespan = DefaultExecutionTimespan;

        private bool isDisposed;

        private TimeSpan maximumLogTimespan = DefaultMaximumLogTimespan;

        #endregion

        #region Constructors

        public DatabaseCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<DatabaseCleanupService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            if (!this.isDisposed)
            {
                this.cancellationTokenSource.Dispose();

                this.isDisposed = true;
            }
        }

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            this.LoadServiceOptions();

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    this.logger.LogDebug("Checking database for outdated entries ...", this.executionTimespan);

                    using var scope = this.serviceScopeFactory.CreateScope();

                    scope.ServiceProvider
                        .GetRequiredService<IErrorLogProvider>()
                        .DeleteOldLogs(this.maximumLogTimespan);

                    scope.ServiceProvider
                        .GetRequiredService<IIOLogProvider>()
                        .DeleteOldLogs(TimeSpan.FromDays(1));       //NOTE keep IO logs shorter (just 1 day)

                    scope.ServiceProvider
                        .GetRequiredService<IMissionLogProvider>()
                        .DeleteOldLogs(this.maximumLogTimespan);

                    scope.ServiceProvider
                       .GetRequiredService<IScreenShotProvider>()
                       .DeleteOldLogs(this.maximumLogTimespan);

                    scope.ServiceProvider
                        .GetRequiredService<IServicingInfoProvider>()
                        .DeleteOldLogs(this.maximumLogTimespan);

                    this.logger.LogDebug("Database cleanup completed.");
                }
                catch (Exception ex) when (ex is OperationCanceledException)
                {
                    this.logger.LogDebug("Stopping service.");

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "An error occurred while cleaning up the database.");
                }
                finally
                {
                    this.logger.LogDebug("Pausing service for {interval}.", this.executionTimespan);

                    await Task.Delay(
                        (int)this.executionTimespan.TotalMilliseconds,
                        this.cancellationTokenSource.Token);
                }
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private void LoadServiceOptions()
        {
            this.logger.LogDebug("Loading service configuration.");

            this.executionTimespan = this.configuration.GetValue("DatabaseCleanup::ExecutionTimespan", DefaultExecutionTimespan);
            this.maximumLogTimespan = this.configuration.GetValue("DatabaseCleanup::MaximumLogTimespan", DefaultMaximumLogTimespan);

            this.configuration
                .GetReloadToken()
                .RegisterChangeCallback(o => this.LoadServiceOptions(), null);

            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion
    }
}
