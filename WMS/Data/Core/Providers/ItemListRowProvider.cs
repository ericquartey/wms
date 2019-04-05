using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemListRowProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.ItemListRows.AddAsync(new Common.DataModels.ItemListRow
            {
                Code = model.Code,
                DispatchedQuantity = model.DispatchedQuantity,
                ItemId = model.ItemId,
                ItemListId = model.ItemListId,
                Lot = model.Lot,
                MaterialStatusId = model.MaterialStatusId,
                PackageTypeId = model.PackageTypeId,
                Priority = model.Priority,
                RegistrationNumber = model.RegistrationNumber,
                RequestedQuantity = model.RequestedQuantity,
                Status = (Common.DataModels.ItemListRowStatus)model.Status,
                Sub1 = model.Sub1,
                Sub2 = model.Sub2
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CompletionDate = entry.Entity.CompletionDate;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastExecutionDate = entry.Entity.LastExecutionDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
            }

            return new SuccessOperationResult<ItemListRowDetails>(model);
        }

        public async Task<IOperationResult<ItemListRowDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListRowDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemListRowDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            this.dataContext.Remove(existingModel);
            await this.dataContext.SaveChangesAsync();
            return new SuccessOperationResult<ItemListRowDetails>();
        }

        public async Task<IEnumerable<ItemListRow>> GetAllAsync(
                    int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<ItemListRow, Common.DataModels.ItemListRow>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                this.SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<ItemListRow, Common.DataModels.ItemListRow>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                       .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                this.SetPolicies(model);
            }

            return model;
        }

        public async Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id)
        {
            return await this.GetAllBase()
                       .Where(l => l.ItemListId == id)
                       .ToArrayAsync();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.ItemListRows,
                       this.GetAllBase());
        }

        public async Task<IOperationResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListRowDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<ItemListRowDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var existingDataModel = this.dataContext.ItemListRows.Find(model.Id);
            this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRowDetails>(model);
        }

        private static Expression<Func<ItemListRow, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (r) =>
                r.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.ItemUnitMeasure.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.MaterialStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.RequestedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                r.Priority.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<ItemListRow> GetAllBase()
        {
            return this.dataContext.ItemListRows
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    Priority = l.Priority,
                    ItemDescription = l.Item.Description,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListId = l.ItemListId,
                    Status = (ItemListRowStatus)l.Status,
                    MaterialStatusDescription = l.MaterialStatus.Description,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description,

                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Status != Common.DataModels.MissionStatus.Completed &&
                        m.Status != Common.DataModels.MissionStatus.Incomplete)
                });
        }

        private IQueryable<ItemListRowDetails> GetAllDetailsBase()
        {
            return this.dataContext.ItemListRows
                .Select(l => new ItemListRowDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    Priority = l.Priority,
                    ItemId = l.Item.Id,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    Status = (ItemListRowStatus)l.Status,
                    ItemDescription = l.Item.Description,
                    CreationDate = l.CreationDate,
                    ItemListCode = l.ItemList.Code,
                    ItemListDescription = l.ItemList.Description,
                    ItemListId = l.ItemListId,
                    ItemListType = (ItemListType)l.ItemList.ItemListType,
                    CompletionDate = l.CompletionDate,
                    LastExecutionDate = l.LastExecutionDate,
                    LastModificationDate = l.LastModificationDate,
                    Lot = l.Lot,
                    RegistrationNumber = l.RegistrationNumber,
                    Sub1 = l.Sub1,
                    Sub2 = l.Sub2,
                    PackageTypeId = l.PackageTypeId,
                    MaterialStatusId = l.MaterialStatusId,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description,

                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Status != Common.DataModels.MissionStatus.Completed &&
                        m.Status != Common.DataModels.MissionStatus.Incomplete)
                });
        }

        #endregion
    }
}
