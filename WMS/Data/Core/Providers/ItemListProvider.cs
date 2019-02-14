using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class ItemListProvider : IItemListProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemListProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<OperationResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.ItemLists.AddAsync(new Common.DataModels.ItemList
            {
                Code = model.Code,
                CustomerOrderCode = model.CustomerOrderCode,
                CustomerOrderDescription = model.CustomerOrderDescription,
                Description = model.Description,
                ItemListType = (Common.DataModels.ItemListType)model.ItemListType,
                Job = model.Job,
                Priority = model.Priority,
                ShipmentUnitAssociated = model.ShipmentUnitAssociated,
                ShipmentUnitCode = model.ShipmentUnitCode,
                ShipmentUnitDescription = model.ShipmentUnitDescription,
                Status = (Common.DataModels.ItemListStatus)model.ItemListStatus,
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
                model.ExecutionEndDate = entry.Entity.ExecutionEndDate;
                model.FirstExecutionDate = entry.Entity.FirstExecutionDate;
            }

            return new SuccessOperationResult<ItemListDetails>(model);
        }

        public async Task<IEnumerable<ItemList>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<ItemList, bool>> whereExpression = null,
            Expression<Func<ItemList, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression)
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(
            Expression<Func<ItemList, bool>> whereExpression = null,
            Expression<Func<ItemList, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(whereExpression, searchExpression)
                       .CountAsync();
        }

        public async Task<ItemListDetails> GetByIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                       .SingleOrDefaultAsync(i => i.Id == id);
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.ItemLists);
        }

        public async Task<OperationResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.ItemLists.Find(model.Id);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemListDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListDetails>(model);
        }

        private IQueryable<ItemList> GetAllBase()
        {
            return this.dataContext.ItemLists
                .Include(i => i.ItemListRows)
                .Select(i => new ItemList
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    ItemListStatus = (ItemListStatus)i.Status,
                    ItemListType = (ItemListType)i.ItemListType,
                    ItemListRowsCount = i.ItemListRows.Count(),
                    ItemListItemsCount = i.ItemListRows.Sum(row => row.RequiredQuantity),
                    CreationDate = i.CreationDate
                });
        }

        private IQueryable<ItemListDetails> GetAllDetailsBase()
        {
            return this.dataContext.ItemLists
                .Include(i => i.ItemListRows)
                .Select(i => new ItemListDetails
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    Priority = i.Priority,
                    ItemListStatus = (ItemListStatus)i.Status,
                    ItemListType = (ItemListType)i.ItemListType,
                    ItemListItemsCount = i.ItemListRows.Sum(row => row.RequiredQuantity),
                    CreationDate = i.CreationDate,
                    Job = i.Job,
                    CustomerOrderCode = i.CustomerOrderCode,
                    CustomerOrderDescription = i.CustomerOrderDescription,
                    ShipmentUnitAssociated = i.ShipmentUnitAssociated,
                    ShipmentUnitCode = i.ShipmentUnitCode,
                    ShipmentUnitDescription = i.ShipmentUnitDescription,
                    LastModificationDate = i.LastModificationDate,
                    FirstExecutionDate = i.FirstExecutionDate,
                    ExecutionEndDate = i.ExecutionEndDate
                });
        }

        #endregion
    }
}
