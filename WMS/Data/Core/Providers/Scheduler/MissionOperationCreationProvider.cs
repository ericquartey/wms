using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionOperationCreationProvider : IMissionOperationCreationProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentOperationProvider compartmentOperationProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILogger<MissionOperationCreationProvider> logger;

        private readonly IMissionLoadingUnitProvider missionLoadingUnitProvider;

        private readonly IMissionOperationProvider missionOperationProvider;

        private readonly IMissionProvider missionProvider;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public MissionOperationCreationProvider(
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            IItemProvider itemProvider,
            ICompartmentOperationProvider compartmentOperationProvider,
            IBayProvider bayProvider,
            IMissionProvider missionProvider,
            IMissionLoadingUnitProvider missionLoadingUnitProvider,
            IMissionOperationProvider missionOperationProvider,
            ILogger<MissionOperationCreationProvider> logger,
            IMapper mapper)
        {
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.itemProvider = itemProvider;
            this.compartmentOperationProvider = compartmentOperationProvider;
            this.bayProvider = bayProvider;
            this.missionProvider = missionProvider;
            this.missionLoadingUnitProvider = missionLoadingUnitProvider;
            this.missionOperationProvider = missionOperationProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<MissionOperation>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (!requests.Any())
            {
                this.logger.LogDebug($"No scheduler requests are available for processing at the moment.");

                return new List<MissionOperation>();
            }

            this.logger.LogDebug($"A total of {requests.Count()} requests need to be processed.");
            var operations = new List<MissionOperation>();
            foreach (var request in requests)
            {
                if (request is ItemSchedulerRequest itemRequest)
                {
                    this.logger.LogDebug($"Scheduler Request (id={request.Id}, type={request.Type}) is the next in line to be processed.");

                    switch (request.OperationType)
                    {
                        case OperationType.Withdrawal:

                            var pickOperations = await this.CreatePickOperationsAsync(itemRequest);
                            operations.AddRange(pickOperations);

                            break;

                        case OperationType.Insertion:

                            var putOperations = await this.CreatePutOperationsAsync(itemRequest);
                            operations.AddRange(putOperations);

                            break;

                        case OperationType.Replacement:
                            this.logger.LogWarning(
                                $"Cannot process scheduler request id={request.Id} because replacement requests are not yet implemented.");
                            break;

                        case OperationType.Reorder:
                            this.logger.LogWarning(
                                $"Cannot process scheduler request id={request.Id} because reorder requests are not yet implemented.");
                            break;

                        default:
                            this.logger.LogError(
                                $"Cannot process scheduler request id={request.Id} because operation type cannot be understood.");
                            break;
                    }
                }
                else if (request is LoadingUnitSchedulerRequest loadingUnitRequest)
                {
                    await this.missionLoadingUnitProvider.CreateWithdrawalOperationAsync(loadingUnitRequest);
                }
                else
                {
                    this.logger.LogError(
                        $"Cannot process scheduler request id={request.Id} because it is not an item scheduler request .");
                }
            }

            return operations;
        }

        public async Task<IEnumerable<MissionOperation>> CreatePickOperationsAsync(ItemSchedulerRequest request)
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
            var missions = new List<Mission>();
            var missionOperations = new List<MissionOperation>();

            while (request.QuantityLeftToReserve > 0
                && availableCompartments.Any()
                && missions.Count < queuableMissionsCount)
            {
                var compartment = availableCompartments.First();

                var quantityToExtractFromCompartment = Math.Min(compartment.Availability, request.QuantityLeftToReserve);
                compartment.ReservedForPick += quantityToExtractFromCompartment;
                request.ReservedQuantity += quantityToExtractFromCompartment;

                await this.compartmentOperationProvider.UpdateAsync(compartment);
                if (request.QuantityLeftToReserve.Equals(0))
                {
                    request.Status = SchedulerRequestStatus.Completed;
                }

                await this.schedulerRequestSchedulerProvider.UpdateAsync(request);

                if (compartment.Availability.Equals(0))
                {
                    availableCompartments.Remove(compartment);
                }

                var mission = await this.missionProvider
                    .GetNewByLoadingUnitIdAsync(compartment.LoadingUnitId);
                if (mission == null)
                {
                    mission = new Mission
                    {
                        BayId = request.BayId.Value,
                        LoadingUnitId = compartment.LoadingUnitId,
                        Priority = request.Priority.Value
                    };

                    missions.Add(mission);
                }

                var operation =
                    request is ItemListRowSchedulerRequest
                    ? new MissionOperation()
                    : new MissionListOperation();

                operation.MissionId = mission.Id;
                operation.ItemId = item.Id;
                operation.CompartmentId = compartment.Id;
                operation.MaterialStatusId = compartment.MaterialStatusId;
                operation.Sub1 = compartment.Sub1;
                operation.Sub2 = compartment.Sub2;
                operation.Priority = request.Priority.Value;
                operation.RequestedQuantity = quantityToExtractFromCompartment;
                operation.Type = MissionOperationType.Pick;

                if (operation is MissionListOperation missionOperation
                    &&
                    request is ItemListRowSchedulerRequest rowRequest)
                {
                    missionOperation.ItemListId = rowRequest.ListId;
                    missionOperation.ItemListRowId = rowRequest.ListRowId;
                }

                this.logger.LogWarning(
                    $"Scheduler Request (id={request.Id}): generating pick mission (CompartmentId={operation.CompartmentId}, " +
                    $"BayId={mission.BayId}, Quantity={operation.RequestedQuantity}). " +
                    $"A total quantity of {request.QuantityLeftToReserve} still needs to be dispatched.");

                missionOperations.Add(operation);
            }

            await this.missionProvider.CreateRangeAsync(missions);
            await this.missionOperationProvider.CreateRangeAsync(missionOperations);

            this.logger.LogDebug(
                $"Scheduler Request (id={request.Id}): a total of {queuableMissionsCount} missions were queued on bay.");

            return missionOperations;
        }

        public async Task<IEnumerable<MissionOperation>> CreatePutOperationsAsync(ItemSchedulerRequest request)
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
            var missions = new List<Mission>();
            var missionOperations = new List<MissionOperation>();

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

                var mission = await this.missionProvider.GetNewByLoadingUnitIdAsync(compartment.LoadingUnitId);
                if (mission == null)
                {
                    mission = new Mission
                    {
                        BayId = request.BayId.Value,
                        LoadingUnitId = compartment.LoadingUnitId,
                        Priority = request.Priority.Value,
                    };

                    missions.Add(mission);
                }

                var operation =
                  request is ItemListRowSchedulerRequest
                  ? new MissionOperation()
                  : new MissionListOperation();

                operation.MissionId = mission.Id;
                operation.ItemId = item.Id;
                operation.CompartmentId = compartment.Id;
                operation.MaterialStatusId = compartment.MaterialStatusId;
                operation.Sub1 = compartment.Sub1;
                operation.Sub2 = compartment.Sub2;
                operation.Priority = request.Priority.Value;
                operation.RequestedQuantity = quantityToPutInCompartment;
                operation.Type = MissionOperationType.Put;

                if (request is ItemListRowSchedulerRequest rowRequest
                    &&
                    operation is MissionListOperation listOperation)
                {
                    listOperation.ItemListId = rowRequest.ListId;
                    listOperation.ItemListRowId = rowRequest.ListRowId;
                }

                this.logger.LogWarning(
                    $"Scheduler Request (id={request.Id}): generating put mission (CompartmentId={operation.CompartmentId}, " +
                    $"BayId={mission.BayId}, Quantity={operation.RequestedQuantity}). " +
                    $"A total quantity of {request.QuantityLeftToReserve} still needs to be dispatched.");

                missionOperations.Add(operation);
            }

            await this.missionProvider.CreateRangeAsync(missions);
            await this.missionOperationProvider.CreateRangeAsync(missionOperations);

            this.logger.LogDebug(
                $"Scheduler Request (id={request.Id}): a total of {queuableMissionsCount} missions were queued on bay.");

            return missionOperations;
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
