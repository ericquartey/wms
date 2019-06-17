using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionCreationProvider : BaseProvider, IMissionCreationProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILogger<MissionCreationProvider> logger;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public MissionCreationProvider(
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            IItemProvider itemProvider,
            ICompartmentOperationProvider compartmentOperationProvider,
            IBayProvider bayProvider,
            DatabaseContext dataContext,
            ILogger<MissionCreationProvider> logger,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.itemProvider = itemProvider;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.logger = logger;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<MissionExecution>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (!requests.Any())
            {
                this.logger.LogDebug($"No scheduler requests are available for processing at the moment.");

                return new List<MissionExecution>();
            }

            this.logger.LogDebug($"A total of {requests.Count()} requests need to be processed.");
            var missions = new List<MissionExecution>();
            foreach (var request in requests)
            {
                this.logger.LogDebug($"Scheduler Request (id={request.Id}, type={request.Type}) is the next in line to be processed.");

                switch (request.OperationType)
                {
                    case OperationType.Withdrawal:
                        {
                            if (request is ItemSchedulerRequest itemRequest)
                            {
                                missions.AddRange(await this.CreatePickMissionsAsync(itemRequest));
                            }
                            else if (request is LoadingUnitSchedulerRequest loadingUnitRequest)
                            {
                                missions.Add(await this.CreateWithdrawalMissionAsync(loadingUnitRequest));
                            }
                            else
                            {
                                throw new InvalidOperationException($"Cannot process scheduler request id={request.Id}.");
                            }
                        }

                        break;

                    case OperationType.Insertion:
                        {
                            if (request is ItemSchedulerRequest itemRequest)
                            {
                                missions.AddRange(await this.CreatePutMissionsAsync(itemRequest));
                            }
                            else
                            {
                                throw new InvalidOperationException($"Cannot process scheduler request id={request.Id}.");
                            }
                        }

                        break;

                    case OperationType.Replacement:
                        this.logger.LogWarning($"Cannot process scheduler request id={request.Id} because replacement requests are not yet implemented.");
                        break;

                    case OperationType.Reorder:
                        this.logger.LogWarning($"Cannot process scheduler request id={request.Id} because reorder requests are not yet implemented.");
                        break;

                    default:
                        this.logger.LogError($"Cannot process scheduler request id={request.Id} because operation type cannot be understood.");
                        break;
                }
            }

            return missions;
        }

        public async Task<IEnumerable<MissionExecution>> CreatePickMissionsAsync(ItemSchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.OperationType != OperationType.Withdrawal)
            {
                throw new InvalidOperationException(
                    $"Pick missions cannot be created for scheduler operation type '{request.OperationType}'.");
            }

            System.Diagnostics.Debug.Assert(
                request.Priority.HasValue,
                "Since a bay is assigned to this request, the priority of the request should be computed as well.");

            var item = await this.itemProvider.GetByIdForExecutionAsync(request.ItemId);

            var candidateCompartments = this.compartmentOperationProvider.GetCandidateCompartments(request);
            var availableCompartments = await this.compartmentOperationProvider
                .OrderCompartmentsByManagementType(candidateCompartments, item.ManagementType, request.OperationType)
                .ToListAsync();

            var queuableMissionsCount = await this.GetQueuableMissionsCountAsync(request);
            var missions = new List<MissionExecution>();

            while (request.QuantityLeftToReserve > 0
                && availableCompartments.Any()
                && missions.Count < queuableMissionsCount)
            {
                var compartment = availableCompartments.First();

                var quantityToExtractFromCompartment = Math.Min(compartment.Availability, request.QuantityLeftToReserve);
                compartment.ReservedForPick += quantityToExtractFromCompartment;
                request.ReservedQuantity += quantityToExtractFromCompartment;

                await this.compartmentOperationProvider.UpdateAsync(compartment);
                if (request.QuantityLeftToReserve.CompareTo(0) == 0)
                {
                    request.Status = SchedulerRequestStatus.Completed;
                }

                await this.schedulerRequestSchedulerProvider.UpdateAsync(request);

                if (compartment.Availability.CompareTo(0) == 0)
                {
                    availableCompartments.Remove(compartment);
                }

                var mission = new MissionExecution
                {
                    ItemId = item.Id,
                    BayId = request.BayId.Value,
                    CellId = compartment.CellId,
                    CompartmentId = compartment.Id,
                    LoadingUnitId = compartment.LoadingUnitId,
                    MaterialStatusId = compartment.MaterialStatusId,
                    Sub1 = compartment.Sub1,
                    Sub2 = compartment.Sub2,
                    Priority = request.Priority.Value,
                    RequestedQuantity = quantityToExtractFromCompartment,
                    Type = MissionType.Pick
                };

                if (request is ItemListRowSchedulerRequest rowRequest)
                {
                    mission.ItemListId = rowRequest.ListId;
                    mission.ItemListRowId = rowRequest.ListRowId;
                }

                this.logger.LogWarning(
                    $"Scheduler Request (id={request.Id}): generating pick mission (CompartmentId={mission.CompartmentId}, " +
                    $"BayId={mission.BayId}, Quantity={mission.RequestedQuantity}). " +
                    $"A total quantity of {request.QuantityLeftToReserve} still needs to be dispatched.");

                missions.Add(mission);
            }

            await this.CreateRangeAsync(missions);

            this.logger.LogDebug(
                $"Scheduler Request (id={request.Id}): a total of {queuableMissionsCount} missions were queued on bay.");

            return missions;
        }

        public async Task<IEnumerable<MissionExecution>> CreatePutMissionsAsync(ItemSchedulerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.OperationType != OperationType.Insertion)
            {
                throw new InvalidOperationException(
                    $"Put missions cannot be created for scheduler operation type '{request.OperationType}'.");
            }

            System.Diagnostics.Debug.Assert(
                request.Priority.HasValue,
                "Since a bay is assigned to this request, the priority of the request should be computed as well.");

            var item = await this.itemProvider.GetByIdForExecutionAsync(request.ItemId);

            var candidateCompartments = this.compartmentOperationProvider.GetCandidateCompartments(request);
            var availableCompartments = await this.compartmentOperationProvider
                .OrderCompartmentsByManagementType(candidateCompartments, item.ManagementType, request.OperationType)
                .ToListAsync();

            var queuableMissionsCount = await this.GetQueuableMissionsCountAsync(request);
            var missions = new List<MissionExecution>();

            while (request.Status != SchedulerRequestStatus.Completed
                && availableCompartments.Any()
                && missions.Count < queuableMissionsCount)
            {
                var compartment = availableCompartments.First();

                System.Diagnostics.Debug.Assert(
                    compartment.RemainingCapacity > 0,
                    "The selected compartments should all have remaining capacity.");

                var quantityToPutInCompartment = Math.Min(compartment.RemainingCapacity, request.QuantityLeftToReserve);
                compartment.ReservedToPut += quantityToPutInCompartment;
                request.ReservedQuantity += quantityToPutInCompartment;

                compartment.ItemId = request.ItemId;
                compartment.Sub1 = request.Sub1;
                compartment.Sub2 = request.Sub2;
                compartment.RegistrationNumber = request.RegistrationNumber;
                compartment.MaterialStatusId = request.MaterialStatusId;
                compartment.Lot = request.Lot;
                compartment.PackageTypeId = request.PackageTypeId;

                await this.compartmentOperationProvider.UpdateAsync(compartment);
                if (request.QuantityLeftToReserve.CompareTo(0) == 0)
                {
                    request.Status = SchedulerRequestStatus.Completed;
                }

                await this.schedulerRequestSchedulerProvider.UpdateAsync(request);

                if (compartment.RemainingCapacity.CompareTo(0) == 0)
                {
                    availableCompartments.Remove(compartment);
                }

                var mission = new MissionExecution
                {
                    ItemId = item.Id,
                    BayId = request.BayId.Value,
                    CellId = compartment.CellId,
                    CompartmentId = compartment.Id,
                    LoadingUnitId = compartment.LoadingUnitId,
                    MaterialStatusId = compartment.MaterialStatusId,
                    Sub1 = compartment.Sub1,
                    Sub2 = compartment.Sub2,
                    Priority = request.Priority.Value,
                    RequestedQuantity = quantityToPutInCompartment,
                    Type = MissionType.Put,
                };

                if (request is ItemListRowSchedulerRequest rowRequest)
                {
                    mission.ItemListId = rowRequest.ListId;
                    mission.ItemListRowId = rowRequest.ListRowId;
                }

                this.logger.LogWarning(
                    $"Scheduler Request (id={request.Id}): generating put mission (CompartmentId={mission.CompartmentId}, " +
                    $"BayId={mission.BayId}, Quantity={mission.RequestedQuantity}). " +
                    $"A total quantity of {request.QuantityLeftToReserve} still needs to be dispatched.");

                missions.Add(mission);
            }

            await this.CreateRangeAsync(missions);

            this.logger.LogDebug(
                $"Scheduler Request (id={request.Id}): a total of {queuableMissionsCount} missions were queued on bay.");

            return missions;
        }

        public async Task<MissionExecution> CreateWithdrawalMissionAsync(LoadingUnitSchedulerRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var mission = new MissionExecution
            {
                BayId = request.BayId,
                LoadingUnitId = request.LoadingUnitId,
                Priority = request.Priority.Value,
                Type = MissionType.Pick
            };

            this.logger.LogWarning(
                $"Scheduler Request (id={request.Id}): generating withdrawal mission (LoadingUnitId={request.LoadingUnitId}, BayId={mission.BayId}. ");

            request.Status = SchedulerRequestStatus.Completed;
            await this.schedulerRequestSchedulerProvider.UpdateAsync(request);

            await this.CreateAsync(mission);

            return mission;
        }

        private async Task CreateAsync(MissionExecution model)
        {
            var mission = new Common.DataModels.Mission
            {
                BayId = model.BayId,
                CellId = model.CellId,
                CompartmentId = model.CompartmentId,
                ItemId = model.ItemId,
                ItemListId = model.ItemListId,
                ItemListRowId = model.ItemListRowId,
                LoadingUnitId = model.LoadingUnitId,
                MaterialStatusId = model.MaterialStatusId,
                PackageTypeId = model.PackageTypeId,
                Priority = model.Priority,
                RegistrationNumber = model.RegistrationNumber,
                RequestedQuantity = model.RequestedQuantity,
                Status = (Common.DataModels.MissionStatus)model.Status,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2,
                Type = (Common.DataModels.MissionType)model.Type
            };

            var entry = await this.DataContext.Missions.AddAsync(mission);

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;

                this.NotificationService.PushCreate(model);

                if (model.ItemId != null)
                {
                    this.NotificationService.PushUpdate(new Item { Id = model.ItemId.Value });
                }

                if (model.LoadingUnitId != null)
                {
                    this.NotificationService.PushUpdate(new LoadingUnit { Id = model.LoadingUnitId.Value });
                }

                if (model.CompartmentId != null)
                {
                    this.NotificationService.PushUpdate(new Compartment { Id = model.CompartmentId.Value });
                }

                if (model.ItemListId != null)
                {
                    this.NotificationService.PushUpdate(new ItemList { Id = model.ItemListId.Value });
                }

                if (model.ItemListRowId != null)
                {
                    this.NotificationService.PushUpdate(new ItemListRow { Id = model.ItemListRowId.Value });
                }
            }
        }

        private async Task CreateRangeAsync(IEnumerable<MissionExecution> models)
        {
            foreach (var model in models)
            {
                await this.CreateAsync(model);
            }
        }

        private async Task<int> GetQueuableMissionsCountAsync(ItemSchedulerRequest request)
        {
            if (!request.BayId.HasValue)
            {
                throw new InvalidOperationException(
                    "Cannot create a pick mission from a request that does not specify the target bay.");
            }

            var bay = await this.bayProvider.GetByIdForExecutionAsync(request.BayId.Value);

            return bay.LoadingUnitsBufferSize.HasValue
                ? bay.LoadingUnitsBufferSize.Value - bay.LoadingUnitsBufferUsage
                : int.MaxValue;
        }

        #endregion
    }
}
