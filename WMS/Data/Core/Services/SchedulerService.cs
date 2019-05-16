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
                try
                {
                    ItemSchedulerRequest qualifiedRequest = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        qualifiedRequest = await requestsExecutionProvider.FullyQualifyPickRequestAsync(itemId, options);
                        if (qualifiedRequest != null)
                        {
                            await requestsExecutionProvider.CreateAsync(qualifiedRequest);

                            transactionScope.Complete();

                            this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for item={qualifiedRequest.ItemId} was accepted and stored.");
                        }
                    }

                    if (qualifiedRequest != null)
                    {
                        await this.ProcessPendingRequestsAsync();

                        return new SuccessOperationResult<ItemSchedulerRequest>(qualifiedRequest);
                    }
                    else
                    {
                        return new BadRequestOperationResult<ItemSchedulerRequest>(qualifiedRequest);
                    }
                }
                catch (System.Exception ex)
                {
                    return new BadRequestOperationResult<ItemSchedulerRequest>(null, ex.Message);
                }
            }
        }

        public async Task<IOperationResult<ItemSchedulerRequest>> PutItemAsync(int itemId, ItemOptions options)
        {
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var requestsExecutionProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestExecutionProvider>();
                try
                {
                    ItemSchedulerRequest qualifiedRequest = null;
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        qualifiedRequest = await requestsExecutionProvider.FullyQualifyPutRequestAsync(itemId, options);
                        if (qualifiedRequest != null)
                        {
                            await requestsExecutionProvider.CreateAsync(qualifiedRequest);

                            transactionScope.Complete();

                            this.logger.LogDebug($"Scheduler Request (id={qualifiedRequest.Id}): Withdrawal for item={qualifiedRequest.ItemId} was accepted and stored.");
                        }
                    }

                    if (qualifiedRequest != null)
                    {
                        await this.ProcessPendingRequestsAsync();

                        return new SuccessOperationResult<ItemSchedulerRequest>(qualifiedRequest);
                    }
                    else
                    {
                        return new BadRequestOperationResult<ItemSchedulerRequest>(qualifiedRequest);
                    }
                }
                catch (System.Exception ex)
                {
                    return new BadRequestOperationResult<ItemSchedulerRequest>(null, ex.Message);
                }
            }
        }

        public async Task<bool> CanPutItemAsync(int itemId, ItemOptions options)
        {
            var errorMessages = new List<string>();

            // TODO: Add Validation to Put Item, check Data
            // INCOMPLETE -> TASK 2711
            using (var serviceScope = this.scopeFactory.CreateScope())
            {
                var itemProvider = serviceScope.ServiceProvider.GetRequiredService<IItemProvider>();
                var schedulerRequestProvider = serviceScope.ServiceProvider.GetRequiredService<ISchedulerRequestProvider>();

                var itemPutDetails = await itemProvider.GetItemPutDetailsAsync(itemId);
                itemPutDetails.RequestedQuantity = options.RequestedQuantity;
                itemPutDetails.QuantityLeftToReserve = schedulerRequestProvider.GetSumQuantityLeftToReserveByItem(itemId);

                if (itemPutDetails.CompartmentsPutDetails.Any())
                {
                    // 1° check: all compartments FILTERED BY USER INPUT
                    FilterByUserInput(itemPutDetails.CompartmentsPutDetails, options);

                    // 2° check: all compartments on MANAGENENT TYPE
                    if (itemPutDetails.ManagementType == ItemManagementType.Volume)
                    {
                        // 2-A° check: all compartments on MANAGENENT TYPE: VOLUME
                        CheckManagementTypeVolume(itemPutDetails, errorMessages);
                    }
                    else if (itemPutDetails.ManagementType == ItemManagementType.FIFO)
                    {
                        // 2-B° check: all compartments on MANAGENENT TYPE: FIFO
                        CheckManagementTypeFifo(itemPutDetails, errorMessages);
                    }

                    return itemPutDetails.CompartmentsPutDetails.Any();
                }
                else
                {
                    errorMessages.Add("No available compartment");
                }
            }

            return true;
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
                catch (System.Exception ex)
                {
                    return new BadRequestOperationResult<LoadingUnitSchedulerRequest>(null, ex.Message);
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

        private static void CheckManagementTypeVolume(ItemPutDetails itemPut, ICollection<string> errorMessages)
        {
            // 3 check : management type of VOLUME
            var compartmentsByVolume = itemPut.CompartmentsPutDetails
                .Where(x =>
                (x.MaxCapacity - x.Stock - x.ReservedToPut) - itemPut.QuantityLeftToReserve >= itemPut.RequestedQuantity);

            if (compartmentsByVolume == null)
            {
                errorMessages.Add("No enough free space in compartments associated");
            }
        }

        private static void CheckManagementTypeFifo(ItemPutDetails itemPut, ICollection<string> errorMessages)
        {
            // 3 check : management type of FIFO
            var compartmentActiveFifo = itemPut.CompartmentsPutDetails.OrderByDescending(
                x => x.FifoStartDate?.AddDays(x.FifoTime).CompareTo(DateTime.Now) > 0);
            var compartmentsEmpty = itemPut.CompartmentsPutDetails.Where(x => x.Stock.Equals(0));

            var compartmentGoodActiveFifo = compartmentActiveFifo.Intersect(compartmentsEmpty);
            if (compartmentGoodActiveFifo == null)
            {
                errorMessages.Add("FIFO Type: No good compartment found with empty stock in active period");
            }
            else
            {
                // Check compartment of VOLUME
                itemPut.CompartmentsPutDetails = compartmentGoodActiveFifo;
                CheckManagementTypeVolume(itemPut, errorMessages);
            }

            var compartmentClosedFifo = itemPut.CompartmentsPutDetails.OrderByDescending(
               x => x.FifoStartDate?.AddDays(x.FifoTime).CompareTo(DateTime.Now) < 0);
            var compartmentGoodClosedFifo = compartmentClosedFifo.Intersect(compartmentsEmpty);
            if (compartmentGoodClosedFifo == null)
            {
                errorMessages.Add("FIFO Type: No good compartment found with empty stock in closed period");
            }
            else
            {
                // Check compartment of VOLUME
                itemPut.CompartmentsPutDetails = compartmentGoodClosedFifo;
                CheckManagementTypeVolume(itemPut, errorMessages);
            }
        }

        private static void FilterByUserInput(IEnumerable<CompartmentPutDetails> compartments, ItemOptions options)
        {
            if (options != null)
            {
                if (options.Lot != null)
                {
                    compartments = compartments.Where(x => x.Lot.Equals(options.Lot, StringComparison.Ordinal));
                }

                if (options.RegistrationNumber != null)
                {
                    compartments = compartments.Where(x => x.RegistrationNumber.Equals(options.RegistrationNumber, StringComparison.Ordinal));
                }

                if (options.Sub1 != null)
                {
                    compartments = compartments.Where(x => x.Sub1.Equals(options.Sub1, StringComparison.Ordinal));
                }

                if (options.Sub2 != null)
                {
                    compartments = compartments.Where(x => x.Sub2.Equals(options.Sub2, StringComparison.Ordinal));
                }

                if (options.MaterialStatusId != null)
                {
                    compartments = compartments.Where(x => x.MaterialStatusId == options.MaterialStatusId);
                }

                if (options.PackageTypeId != null)
                {
                    compartments = compartments.Where(x => x.PackageTypeId == options.PackageTypeId);
                }
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
