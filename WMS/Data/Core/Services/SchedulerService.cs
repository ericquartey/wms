using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "Ok")]
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

        public async Task<IOperationResult<ItemSchedulerRequest>> PickItemAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var requestsPickProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestPickProvider>();

                try
                {
                    IOperationResult<ItemSchedulerRequest> result = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        result = await requestsPickProvider.FullyQualifyPickRequestAsync(itemId, options);
                        if (result.Success)
                        {
                            var createResult = await requestsExecutionProvider.CreateAsync(result.Entity);
                            if (!createResult.Success)
                            {
                                return createResult;
                            }

                            transactionScope.Complete();

                            this.logger.LogDebug($"Scheduler Request (id={result.Entity.Id}): Pick for item={result.Entity.ItemId} was accepted and stored.");
                        }
                    }

                    if (result.Success)
                    {
                        await this.ProcessPendingRequestsAsync();

                        return new SuccessOperationResult<ItemSchedulerRequest>(result.Entity);
                    }
                    else
                    {
                        return new BadRequestOperationResult<ItemSchedulerRequest>(result.Entity);
                    }
                }
                catch (System.Exception ex)
                {
                    return new BadRequestOperationResult<ItemSchedulerRequest>(null, ex.Message);
                }
            }
        }

        public async Task<IOperationResult<double>> GetPutCapacityAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsPutProvider = serviceScope
                    .ServiceProvider
                    .GetRequiredService<ISchedulerRequestPutProvider>();

                try
                {
                    return await requestsPutProvider.GetAvailableCapacityAsync(itemId, options);
                }
                catch (Exception ex)
                {
                    return new BadRequestOperationResult<double>(ex);
                }
            }
        }

        public async Task<IOperationResult<ItemSchedulerRequest>> PutItemAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var requestsPutProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestPutProvider>();

                try
                {
                    IOperationResult<ItemSchedulerRequest> result = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        result = await requestsPutProvider.FullyQualifyPutRequestAsync(itemId, options);
                        if (result.Success)
                        {
                            var qualifiedRequest = result.Entity;
                            var createResult = await requestsExecutionProvider.CreateAsync(qualifiedRequest);
                            if (!createResult.Success)
                            {
                                return createResult;
                            }

                            transactionScope.Complete();

                            this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Put for item={qualifiedRequest.ItemId} was accepted and stored.");
                        }
                    }

                    if (result.Success)
                    {
                        await this.ProcessPendingRequestsAsync();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    return new BadRequestOperationResult<ItemSchedulerRequest>(ex);
                }
            }
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId, int bayId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                try
                {
                    LoadingUnitSchedulerRequest qualifiedRequest = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        qualifiedRequest = new LoadingUnitSchedulerRequest
                        {
                            IsInstant = true,
                            LoadingUnitId = loadingUnitId,
                            LoadingUnitTypeId = loadingUnitTypeId,
                            Priority = 1,
                            BayId = bayId,
                            Status = SchedulerRequestStatus.New,
                        };

                        var createdRequest = await requestsExecutionProvider.CreateAsync(qualifiedRequest);
                        qualifiedRequest = createdRequest.Entity;
                        transactionScope.Complete();

                        this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for loadingUnit={qualifiedRequest.LoadingUnitId} was accepted and stored.");
                    }

                    await this.ProcessPendingRequestsAsync();
                    return new SuccessOperationResult<LoadingUnitSchedulerRequest>(qualifiedRequest);
                }
                catch (Exception ex)
                {
                    return new BadRequestOperationResult<LoadingUnitSchedulerRequest>(ex);
                }
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var listsProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListExecutionProvider>();

                var result = await listsProvider.PrepareForExecutionAsync(listId, areaId, bayId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<ItemList>> SuspendListAsync(int id)
        {
            this.logger.LogDebug($"Suspending execution of list id={id}.");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var listProvider = scope.ServiceProvider.GetRequiredService<IItemListExecutionProvider>();

                await listProvider.SuspendAsync(id);
            }

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int id)
        {
            this.logger.LogDebug($"Suspending execution of list row id={id}.");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var rowProvider = scope.ServiceProvider.GetRequiredService<IItemListRowExecutionProvider>();

                await rowProvider.SuspendAsync(id);
            }

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<MissionExecution>> CompleteItemMissionAsync(int missionId, double quantity)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionExecutionProvider>();

                var result = await missionsProvider.CompleteItemAsync(missionId, quantity);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<MissionExecution>> CompleteLoadingUnitMissionAsync(int missionId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionExecutionProvider>();

                var result = await missionsProvider.CompleteLoadingUnitAsync(missionId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<ItemListRowSchedulerRequest>> ExecuteListRowAsync(int rowId, int areaId, int? bayId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var listRowProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListRowExecutionProvider>();

                var result = await listRowProvider.PrepareForExecutionAsync(rowId, areaId, bayId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<MissionExecution>> ExecuteMissionAsync(int missionId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionExecutionProvider>();

                return await missionsProvider.ExecuteAsync(missionId);
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
                var requestsProvider = scope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var missionsProvider = scope.ServiceProvider.GetRequiredService<IMissionExecutionProvider>();

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

                        await this.SeedDatabaseAsync(database, stoppingToken);
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

        private async Task SeedDatabaseAsync(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, CancellationToken stoppingToken)
        {
            try
            {
                this.logger.LogDebug($"Reseeding database (Dev.Minimal.sql) ...");
                var minimalDbScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.2\win7-x64\Seeds\Dev.Minimal.sql");
                await database.ExecuteSqlCommandAsync(minimalDbScript);

                this.logger.LogDebug($"Reseeding database (Dev.Items.sql) ...");
                var itemsScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.2\win7-x64\Seeds\Dev.Items.sql");
                await database.ExecuteSqlCommandAsync(itemsScript);
            }
            catch (System.Exception ex)
            {
                this.logger.LogCritical($"Unable to seed database: {ex.Message}");
                await this.StopAsync(stoppingToken);
            }
        }

        #endregion
    }
}
