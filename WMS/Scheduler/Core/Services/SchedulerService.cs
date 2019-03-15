using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.Core.Services
{
    internal class SchedulerService : BackgroundService, ISchedulerService
    {
        private readonly ILogger<SchedulerService> logger;

        private readonly IServiceScopeFactory scopeFactory;

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
        }

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.CheckDatabaseStatusAsync(cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        public async Task<SchedulerRequest> WithdrawItemAsync(SchedulerRequest request)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestProvider>();

                SchedulerRequest qualifiedRequest = null;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    qualifiedRequest = await requestsProvider.FullyQualifyWithdrawalRequestAsync(request);
                    if (qualifiedRequest != null)
                    {
                        await requestsProvider.CreateAsync(qualifiedRequest);

                        transactionScope.Complete();
                        this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for item={qualifiedRequest.ItemId} was accepted and stored.");
                    }
                }

                await this.ProcessPendingRequestsAsync();

                return qualifiedRequest;
            }
        }

        public async Task<IEnumerable<SchedulerRequest>> ExecuteListAsync(ListExecutionRequest request)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var listsProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListSchedulerProvider>();

                var acceptedRequests = await listsProvider.PrepareForExecutionAsync(request);

                await this.ProcessPendingRequestsAsync();

                return acceptedRequests;
            }
        }

        public async Task<IOperationResult<Mission>> CompleteMissionAsync(int id, int quantity)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionSchedulerProvider>();

                var result = await missionsProvider.CompleteAsync(id, quantity);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<SchedulerRequest>> ExecuteListRowAsync(ListRowExecutionRequest request)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var listRowProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListRowSchedulerProvider>();

                return await listRowProvider.PrepareForExecutionAsync(request);
            }
        }

        public async Task<IOperationResult<Mission>> ExecuteMissionAsync(int id)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionSchedulerProvider>();

                return await missionsProvider.ExecuteAsync(id);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await this.ProcessPendingRequestsAsync();
            }
            catch
            {
                this.logger.LogWarning("Scheduler start-up request processing failed.");
                await this.StopAsync(stoppingToken);
            }
        }

        private async Task ProcessPendingRequestsAsync()
        {
            this.logger.LogDebug("Checking for pending scheduler requests to process ...");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = scope.ServiceProvider.GetRequiredService<ISchedulerRequestProvider>();
                var missionsProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulerProvider>();

                var requests = await requestsProvider.GetRequestsToProcessAsync();
                await missionsProvider.CreateForRequestsAsync(requests);
            }

            this.logger.LogDebug("Done processing pending requests.");
        }

        private async Task CheckDatabaseStatusAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = this.scopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    var database = databaseContext.Database;

                    this.logger.LogDebug("Checking database structure ...");

                    var pendingMigrations = await database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        this.logger.LogInformation($"A total of {pendingMigrations.Count()} pending migrations found.");
#if DEBUG
                        this.logger.LogDebug($"Applying migrations ...");

                        await database.MigrateAsync();

                        this.logger.LogDebug($"Reseeding database ...");

                        var cleanAll = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.CleanAll.sql");
                        var initDbScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.InitDb.sql");
                        var itemsScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.1\Seeds\Dev.Items.sql");

                        await database.ExecuteSqlCommandAsync(cleanAll);
                        await database.ExecuteSqlCommandAsync(initDbScript);
                        await database.ExecuteSqlCommandAsync(itemsScript);
#else
                        this.logger.LogCritical("Database is not up to date. Please apply the migrations and restart the service.");
                        await this.StopAsync(stoppingToken);
#endif
                    }
                    else
                    {
                        this.logger.LogDebug($"Database is up to date.");
                    }
                }
            }
            catch
            {
                this.logger.LogCritical("Unable to check database structure.");
                await this.StopAsync(stoppingToken);
            }
        }

        #endregion
    }
}
