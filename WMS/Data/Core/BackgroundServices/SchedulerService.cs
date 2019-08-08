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
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Services
{
    internal class SchedulerService :
        BackgroundService,
        IMissionSchedulerService,
        ISchedulerService,
        IItemSchedulerService,
        IItemListSchedulerService
    {
        #region Fields

        private readonly IApplicationLifetime appLifetime;

        private readonly IHostingEnvironment environment;

        private readonly ILogger<SchedulerService> logger;

        private readonly IServiceScopeFactory scopeFactory;

        #endregion

        #region Constructors

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IServiceScopeFactory scopeFactory,
            IApplicationLifetime appLifetime,
            IHostingEnvironment environment)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            this.appLifetime = appLifetime;
            this.environment = environment;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<MissionOperation>> AbortOperationAsync(int operationId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var operationsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionOperationProvider>();

                var result = await operationsProvider.AbortAsync(operationId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<Mission>> CompleteLoadingUnitMissionAsync(int missionId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionLoadingUnitProvider>();

                var result = await missionsProvider.CompleteAsync(missionId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<MissionOperation>> CompleteOperationAsync(int operationId, double quantity)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var operationsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionOperationProvider>();

                var result = await operationsProvider.CompleteAsync(operationId, quantity);

                await this.ProcessPendingRequestsAsync();

                return result;
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

        public async Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListRowAsync(int rowId, int areaId, int? bayId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var listRowProvider = serviceScope.ServiceProvider.GetRequiredService<IItemListRowExecutionProvider>();

                var result = await listRowProvider.PrepareForExecutionAsync(rowId, areaId, bayId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<Mission>> ExecuteLoadingUnitMissionAsync(int missionId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var missionsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionLoadingUnitProvider>();

                var result = await missionsProvider.ExecuteAsync(missionId);

                await this.ProcessPendingRequestsAsync();

                return result;
            }
        }

        public async Task<IOperationResult<MissionOperation>> ExecuteOperationAsync(int operationId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var operationsProvider = serviceScope.ServiceProvider.GetRequiredService<IMissionOperationProvider>();

                return await operationsProvider.ExecuteAsync(operationId);
            }
        }

        public async Task<IOperationResult<double>> GetPickAvailabilityAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = serviceScope
                    .ServiceProvider
                    .GetRequiredService<ISchedulerRequestPickProvider>();

                return await requestsProvider.GetItemAvailabilityAsync(itemId, options);
            }
        }

        public async Task<IOperationResult<double>> GetPutCapacityAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = serviceScope
                    .ServiceProvider
                    .GetRequiredService<ISchedulerRequestPutProvider>();

                return await requestsProvider.GetAvailableCapacityAsync(itemId, options);
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PickItemAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var requestsPickProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestPickProvider>();

                try
                {
                    IOperationResult<IEnumerable<ItemSchedulerRequest>> result = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        result = await requestsPickProvider.FullyQualifyPickRequestAsync(itemId, options);
                        if (result.Success)
                        {
                            var createResult = await requestsExecutionProvider.CreateRangeAsync(result.Entity);
                            if (!createResult.Success)
                            {
                                return createResult;
                            }

                            transactionScope.Complete();

                            this.logger.LogDebug($"Pick request for item={itemId} was accepted.");
                        }
                    }

                    if (result.Success)
                    {
                        await this.ProcessPendingRequestsAsync();

                        return new SuccessOperationResult<IEnumerable<ItemSchedulerRequest>>(result.Entity);
                    }
                    else
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(ex);
                }
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemSchedulerRequest>>> PutItemAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var requestsPutProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestPutProvider>();

                try
                {
                    IOperationResult<IEnumerable<ItemSchedulerRequest>> result;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        result = await requestsPutProvider.FullyQualifyPutRequestAsync(itemId, options);
                        if (result.Success)
                        {
                            var qualifiedRequests = result.Entity;
                            var createResult = await requestsExecutionProvider.CreateRangeAsync(qualifiedRequests);
                            if (!createResult.Success)
                            {
                                return createResult;
                            }

                            transactionScope.Complete();

                            this.logger.LogDebug($"Put request for item={itemId} was accepted and stored.");
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
                    return new BadRequestOperationResult<IEnumerable<ItemSchedulerRequest>>(ex);
                }
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.CheckDatabaseStatusAsync(cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        public async Task<IOperationResult<ItemList>> SuspendListAsync(int listId)
        {
            this.logger.LogDebug($"Suspending execution of list id={listId}.");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var listProvider = scope.ServiceProvider.GetRequiredService<IItemListExecutionProvider>();

                await listProvider.SuspendAsync(listId);
            }

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int listId)
        {
            this.logger.LogDebug($"Suspending execution of list row id={listId}.");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var rowProvider = scope.ServiceProvider.GetRequiredService<IItemListRowExecutionProvider>();

                await rowProvider.SuspendAsync(listId);
            }

            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<LoadingUnitSchedulerRequest>> WithdrawLoadingUnitAsync(int loadingUnitId, int loadingUnitTypeId, int bayId)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                try
                {
                    LoadingUnitSchedulerRequest qualifiedRequest = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        qualifiedRequest = new LoadingUnitSchedulerRequest
                        {
                            IsInstant = true,
                            LoadingUnitId = loadingUnitId,
                            Priority = 1,
                            BayId = bayId,
                            Status = Enums.SchedulerRequestStatus.New,
                        };

                        var creationResult = await requestsProvider.CreateAsync(qualifiedRequest);
                        if (!creationResult.Success)
                        {
                            return creationResult;
                        }

                        qualifiedRequest = creationResult.Entity;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await this.ProcessPendingRequestsAsync();
            }
            catch
            {
                this.logger.LogWarning("Scheduler start-up request processing failed.");
                this.appLifetime.StopApplication();
            }
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

                        if (this.environment.IsProduction())
                        {
                            this.logger.LogCritical("Database is not up to date. Please apply the migrations and restart the service.");
                            this.appLifetime.StopApplication();
                        }
                        else
                        {
                            this.logger.LogDebug($"Applying migrations ...");

                            await database.MigrateAsync();

                            await this.SeedDatabaseAsync(database, stoppingToken);
                        }
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
                this.appLifetime.StopApplication();
            }
        }

        private async Task ProcessPendingRequestsAsync()
        {
            this.logger.LogDebug("Checking for pending scheduler requests to process ...");

            using (var scope = this.scopeFactory.CreateScope())
            {
                var requestsProvider = scope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                var operationsProvider = scope.ServiceProvider.GetRequiredService<IMissionOperationCreationProvider>();

                var requests = await requestsProvider.GetRequestsToProcessAsync();
                await operationsProvider.CreateForRequestsAsync(requests);
            }

            this.logger.LogDebug("Done processing pending requests.");
        }

        private async Task SeedDatabaseAsync(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogDebug($"Reseeding database (Dev.Minimal.sql) ...");
                var minimalDbScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.2\win7-x64\Seeds\Dev.Minimal.sql");
                await database.ExecuteSqlCommandAsync(minimalDbScript, cancellationToken);

                this.logger.LogDebug($"Reseeding database (Dev.Items.sql) ...");
                var itemsScript = await System.IO.File.ReadAllTextAsync(@"bin\Debug\netcoreapp2.2\win7-x64\Seeds\Dev.Items.sql");
                await database.ExecuteSqlCommandAsync(itemsScript, cancellationToken);
            }
            catch (System.Exception ex)
            {
                this.logger.LogCritical($"Unable to seed database: {ex.Message}");
                this.appLifetime.StopApplication();
            }
        }

        #endregion
    }
}
