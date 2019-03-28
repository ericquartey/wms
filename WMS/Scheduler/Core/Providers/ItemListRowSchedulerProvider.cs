using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class ItemListRowSchedulerProvider : IItemListRowSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        private readonly IMissionSchedulerProvider missionProvider;

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public ItemListRowSchedulerProvider(
            DatabaseContext databaseContext,
            IMissionSchedulerProvider missionProvider,
            ISchedulerRequestProvider schedulerRequestProvider)
        {
            this.databaseContext = databaseContext;
            this.missionProvider = missionProvider;
            this.schedulerRequestProvider = schedulerRequestProvider;
        }

        #endregion

        #region Methods

        public async Task<ItemListRow> GetByIdAsync(int id)
        {
            return await this.databaseContext.ItemListRows
                .Select(r => new ItemListRow
                {
                    Id = r.Id,
                    ListId = r.ItemListId,
                    Status = (ListRowStatus)r.Status,
                    DispatchedQuantity = r.DispatchedQuantity,
                    Priority = r.Priority
                })
                .SingleAsync(i => i.Id == id);
        }

        public async Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(
            int id,
            int areaId,
            int? bayId)
        {
            var row = await this.GetByIdAsync(id);

            return await this.ExecutionAsync(row, areaId, bayId);
        }

        public async Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(
            ItemListRow row,
            int areaId,
            int? bayId)
        {
            return await this.ExecutionAsync(row, areaId, bayId);
        }

        public async Task<IOperationResult<ItemListRow>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new System.NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRow>> UpdateAsync(ItemListRow model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingRow = this.databaseContext.ItemListRows.Find(model.Id);
            this.databaseContext.Entry(existingRow).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRow>(model);
        }

        public async Task UpdateRowStatusAsync(ItemListRow row, System.DateTime now)
        {
            var currentStatus = row.Status;
            var involvedMissions = await this.missionProvider.GetByListRowIdAsync(row.Id);

            var completeMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Completed);
            var waitingMissionsCount = involvedMissions.Count(m => m.Status == MissionStatus.Waiting);
            var hasExecutingMissions = involvedMissions.Any(m => m.Status == MissionStatus.Executing);
            var hasErroredMissions = involvedMissions.Any(m => m.Status == MissionStatus.Error);
            var hasIncompleteMissions = involvedMissions.Any(m => m.Status == MissionStatus.Incomplete);

            if (completeMissionsCount == involvedMissions.Count()
                && involvedMissions.Sum(m => m.DispatchedQuantity) == row.RequestedQuantity)
            {
                row.Status = ListRowStatus.Completed;
                row.CompletionDate = now;
            }
            else if (waitingMissionsCount == involvedMissions.Count())
            {
                row.Status = ListRowStatus.Waiting;
            }
            else if (hasErroredMissions)
            {
                row.Status = ListRowStatus.Error;
            }
            else if (hasExecutingMissions)
            {
                row.Status = ListRowStatus.Executing;
                row.LastExecutionDate = now;
            }
            else if (hasIncompleteMissions)
            {
                row.Status = ListRowStatus.Incomplete;
            }

            if (currentStatus != row.Status)
            {
                await this.UpdateAsync(row);
            }
        }

        private async Task<IOperationResult<SchedulerRequest>> ExecutionAsync(
            ItemListRow row,
            int areaId,
            int? bayId)
        {
            var options = new ItemWithdrawOptions
            {
                RunImmediately = false,
                BayId = bayId,
                AreaId = areaId,
                RequestedQuantity = row.RequestedQuantity,
                Lot = row.Lot,
                MaterialStatusId = row.MaterialStatusId,
                PackageTypeId = row.PackageTypeId,
                RegistrationNumber = row.RegistrationNumber,
                Sub1 = row.Sub1,
                Sub2 = row.Sub2,
            };

            var qualifiedRequest = await this.schedulerRequestProvider
                .FullyQualifyWithdrawalRequestAsync(row.ItemId, options);

            if (qualifiedRequest != null)
            {
                qualifiedRequest.ListId = row.ListId;
                qualifiedRequest.ListRowId = row.Id;

                row.Status = bayId.HasValue
                    ? ListRowStatus.Executing
                    : ListRowStatus.Waiting;
            }
            else
            {
                row.Status = ListRowStatus.Waiting;
            }

            await this.UpdateRowStatusAsync(row, System.DateTime.UtcNow);

            await this.UpdateAsync(row);

            if (qualifiedRequest != null)
            {
                return new SuccessOperationResult<SchedulerRequest>(qualifiedRequest);
            }
            else
            {
                return new BadRequestOperationResult<SchedulerRequest>(null);
            }
        }

        #endregion
    }
}
