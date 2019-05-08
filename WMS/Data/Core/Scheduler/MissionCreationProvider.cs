using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class MissionCreationProvider : IMissionCreationProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ICompartmentExecutionProvider compartmentSchedulerProvider;

        private readonly DatabaseContext dataContext;

        private readonly IItemProvider itemProvider;

        private readonly ILogger<MissionCreationProvider> logger;

        private readonly ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider;

        #endregion

        #region Constructors

        public MissionCreationProvider(
            ISchedulerRequestExecutionProvider schedulerRequestSchedulerProvider,
            IItemProvider itemProvider,
            ICompartmentExecutionProvider compartmentSchedulerProvider,
            IBayProvider bayProvider,
            DatabaseContext dataContext,
            ILogger<MissionCreationProvider> logger)
        {
            this.schedulerRequestSchedulerProvider = schedulerRequestSchedulerProvider;
            this.itemProvider = itemProvider;
            this.compartmentSchedulerProvider = compartmentSchedulerProvider;
            this.logger = logger;
            this.bayProvider = bayProvider;
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

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

        public async Task<IEnumerable<MissionExecution>> CreateWithdrawalMissionsAsync(ItemSchedulerRequest request)
        {
            if (request == null)
            {
                return null;
            }

            if (request.BayId.HasValue == false)
            {
                throw new InvalidOperationException(
                    "Cannot create a withdrawal mission from a request that does not specify the target bay.");
            }

            System.Diagnostics.Debug.Assert(
                request.Priority.HasValue,
                "Since a bay is assigned to this request, the priority of the request should be computed as well.");

            var item = await this.itemProvider.GetByIdForExecutionAsync(request.ItemId);

            var bay = await this.bayProvider.GetByIdForExecutionAsync(request.BayId.Value);

            var queuableMissionsCount = bay.LoadingUnitsBufferSize.HasValue
                ? bay.LoadingUnitsBufferSize.Value - bay.LoadingUnitsBufferUsage
                : int.MaxValue;

            var candidateCompartments = this.compartmentSchedulerProvider.GetCandidateWithdrawalCompartments(request);
            var availableCompartments = await this.compartmentSchedulerProvider
                .OrderPickCompartmentsByManagementType(candidateCompartments, item.ManagementType)
                .ToListAsync();

            var missions = new List<MissionExecution>();
            while (request.QuantityLeftToReserve > 0
                && availableCompartments.Any()
                && missions.Count < queuableMissionsCount)
            {
                var compartment = availableCompartments.First();

                var quantityToExtractFromCompartment = Math.Min(compartment.Availability, request.QuantityLeftToReserve);
                compartment.ReservedForPick += quantityToExtractFromCompartment;
                request.ReservedQuantity += quantityToExtractFromCompartment;

                await this.compartmentSchedulerProvider.UpdateAsync(compartment);
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
                    $"Scheduler Request (id={request.Id}): generating withdrawal mission (CompartmentId={mission.CompartmentId}, " +
                    $"BayId={mission.BayId}, Quantity={mission.RequestedQuantity}). " +
                    $"A total quantity of {request.QuantityLeftToReserve} still needs to be dispatched.");

                missions.Add(mission);
            }

            await this.CreateRangeAsync(missions);

            this.logger.LogDebug($"Scheduler Request (id={request.Id}): a total of {queuableMissionsCount} were queued on bay (id={bay.Id}).");

            return missions;
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

            await this.dataContext.Missions.AddAsync(mission);

            await this.dataContext.SaveChangesAsync();
        }

        private async Task CreateRangeAsync(IEnumerable<MissionExecution> models)
        {
            var missions = models.Select(
                m => new Common.DataModels.Mission
                {
                    BayId = m.BayId,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,
                    Priority = m.Priority,
                    RegistrationNumber = m.RegistrationNumber,
                    RequestedQuantity = m.RequestedQuantity,
                    Status = (Common.DataModels.MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (Common.DataModels.MissionType)m.Type
                });

            await this.dataContext.Missions.AddRangeAsync(missions);

            await this.dataContext.SaveChangesAsync();
        }

        #endregion
    }
}
