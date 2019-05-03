using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
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
    internal partial class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public ItemProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.Items.AddAsync(new Common.DataModels.Item
            {
                AbcClassId = model.AbcClassId,
                AverageWeight = model.AverageWeight,
                Code = model.Code,
                Description = model.Description,
                FifoTimePick = model.FifoTimePick,
                FifoTimeStore = model.FifoTimeStore,
                Height = model.Height,
                Image = model.Image,
                InventoryDate = model.InventoryDate,
                InventoryTolerance = model.InventoryTolerance,
                ItemCategoryId = model.ItemCategoryId,
                LastPickDate = model.LastPickDate,
                LastStoreDate = model.LastStoreDate,
                Length = model.Length,
                ManagementType = (Common.DataModels.ItemManagementType)model.ManagementType,
                MeasureUnitId = model.MeasureUnitId,
                Note = model.Note,
                PickTolerance = model.PickTolerance,
                ReorderPoint = model.ReorderPoint,
                ReorderQuantity = model.ReorderQuantity,
                StoreTolerance = model.StoreTolerance,
                Width = model.Width
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
            }

            return new SuccessOperationResult<ItemDetails>(model);
        }

        public async Task<IOperationResult<ItemDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<ItemDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            return await this.DeleteWithRelatedDataAsync(existingModel);
        }

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<Item, Common.DataModels.Item>(
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
                .CountAsync<Item, Common.DataModels.Item>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<IEnumerable<Item>> GetByAreaIdAsync(
            int areaId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return await this.GetFilteredItemByArea(areaId)
                .ToArrayAsync<Item, Common.DataModels.Item>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                this.SetPolicies(model);
            }

            return model;
        }

        public async Task<ItemScheduler> GetByIdSchedulerAsync(int id)
        {
            return await this.dataContext.Items
               .Select(i => new ItemScheduler
               {
                   Id = i.Id,
                   ManagementType = (ItemManagementType)i.ManagementType,
                   LastPickDate = i.LastPickDate
               })
               .SingleAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.dataContext.Items,
                this.GetAllBase());
        }

        public async Task<IOperationResult<ItemScheduler>> UpdateAsync(ItemScheduler model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Items.Find(model.Id);
            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemScheduler>(model);
        }

        public async Task<IOperationResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<ItemDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<ItemDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var existingDataModel = this.dataContext.Items.Find(model.Id);
            this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemDetails>(model);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);

            return (i) =>
                (i.AbcClassDescription != null && i.AbcClassDescription.Contains(search))
                || (i.Code != null && i.Code.Contains(search))
                || (i.Description != null && i.Description.Contains(search))
                || (i.ItemCategoryDescription != null && i.ItemCategoryDescription.Contains(search))
                || (i.MeasureUnitDescription != null && i.MeasureUnitDescription.Contains(search))
                || (successConversionAsDouble
                    && (Equals(i.TotalAvailable, searchAsDouble)
                        || Equals(i.TotalReservedForPick, searchAsDouble)
                        || Equals(i.TotalReservedToStore, searchAsDouble)
                        || Equals(i.TotalStock, searchAsDouble)));
        }

        private async Task<OperationResult<ItemDetails>> DeleteWithRelatedDataAsync(ItemDetails model)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var existingModel = this.dataContext.Items.Find(model.Id);
                if (existingModel == null)
                {
                    return new NotFoundOperationResult<ItemDetails>();
                }

                var areaCount =
                    await this.dataContext.ItemsAreas
                        .CountAsync(c => c.ItemId == model.Id);

                var compartmentTypeCount =
                    await this.dataContext.ItemsAreas
                        .CountAsync(c => c.ItemId == model.Id);

                if (areaCount > 0)
                {
                    var area = await this.dataContext.ItemsAreas
                        .Where(a => a.ItemId == model.Id)
                        .ToListAsync();
                    this.dataContext.RemoveRange(area);
                }

                if (compartmentTypeCount > 0)
                {
                    var compartmentType = await this.dataContext.ItemsCompartmentTypes
                        .Where(t => t.ItemId == model.Id)
                        .ToListAsync();
                    this.dataContext.RemoveRange(compartmentType);
                }

                this.dataContext.Remove(existingModel);
                await this.dataContext.SaveChangesAsync();
                scope.Complete();

                return new SuccessOperationResult<ItemDetails>(model);
            }
        }

        private IQueryable<Item> GetAllBase(
            Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.dataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.dataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToStore = j.Sum(x => x.ReservedToStore)
                        }),
                    i => i.Id,
                    c => c.ItemId,
                    (i, c) => new
                    {
                        Item = i,
                        CompartmentsAggregation = c
                    })
                .SelectMany(
                    temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                    (i, c) => new Item
                    {
                        Id = i.Item.Id,
                        AbcClassId = i.Item.AbcClassId,
                        AverageWeight = i.Item.AverageWeight,
                        CreationDate = i.Item.CreationDate,
                        FifoTimePick = i.Item.FifoTimePick,
                        FifoTimeStore = i.Item.FifoTimeStore,
                        Height = i.Item.Height,
                        Image = i.Item.Image,
                        InventoryDate = i.Item.InventoryDate,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        ManagementType = (ItemManagementType)i.Item.ManagementType,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastStoreDate = i.Item.LastStoreDate,
                        Length = i.Item.Length,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        PickTolerance = i.Item.PickTolerance,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,
                        StoreTolerance = i.Item.StoreTolerance,
                        Width = i.Item.Width,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        TotalStock = c != null ? c.TotalStock : 0,
                        TotalReservedForPick = c != null ? c.TotalReservedForPick : 0,
                        TotalReservedToStore = c != null ? c.TotalReservedToStore : 0,
                        ItemCategoryId = i.Item.ItemCategoryId,
                        ItemCategoryDescription = i.Item.ItemCategory.Description,
                        AbcClassDescription = i.Item.AbcClass.Description,

                        TotalAvailable =
                            c != null
                                ? c.TotalStock + c.TotalReservedToStore - c.TotalReservedForPick
                                : 0,

                        CompartmentsCount = i.Item.Compartments.Count(),
                        MissionsCount = i.Item.Missions.Count(),
                        SchedulerRequestsCount = i.Item.SchedulerRequests.Count(),
                        ItemListRowsCount = i.Item.ItemListRows.Count(),
                    });
        }

        private IQueryable<ItemDetails> GetAllDetailsBase(
            Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.dataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.dataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToStore = j.Sum(x => x.ReservedToStore)
                        }),
                    i => i.Id,
                    c => c.ItemId,
                    (i, c) => new
                    {
                        Item = i,
                        CompartmentsAggregation = c
                    })
                .SelectMany(
                    temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                    (i, c) => new ItemDetails
                    {
                        Id = i.Item.Id,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        ItemCategoryId = i.Item.ItemCategoryId,
                        Note = i.Item.Note,

                        AbcClassId = i.Item.AbcClassId,
                        MeasureUnitId = i.Item.MeasureUnitId,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        ManagementType = (ItemManagementType)i.Item.ManagementType,
                        FifoTimePick = i.Item.FifoTimePick,
                        FifoTimeStore = i.Item.FifoTimeStore,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,

                        Height = i.Item.Height,
                        Length = i.Item.Length,
                        Width = i.Item.Width,
                        PickTolerance = i.Item.PickTolerance,
                        StoreTolerance = i.Item.StoreTolerance,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        AverageWeight = i.Item.AverageWeight,

                        Image = i.Item.Image,

                        CreationDate = i.Item.CreationDate,
                        InventoryDate = i.Item.InventoryDate,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastStoreDate = i.Item.LastStoreDate,

                        TotalAvailable =
                            c != null
                                ? c.TotalStock + c.TotalReservedToStore - c.TotalReservedForPick
                                : 0,

                        CompartmentsCount = i.Item.Compartments.Count(),
                        MissionsCount = i.Item.Missions.Count(),
                        SchedulerRequestsCount = i.Item.SchedulerRequests.Count(),
                        ItemListRowsCount = i.Item.ItemListRows.Count(),
                    });
        }

        private IQueryable<Item> GetFilteredItemByArea(int areaId)
        {
            return this.dataContext.Compartments
                .Select(c => new
                {
                    Item = c.Item,
                    Aisle = c.LoadingUnit.Cell.Aisle,
                    Quantity = c.Stock,
                })
                .Where(x => x.Aisle.AreaId == areaId)
                .Join(
                    this.dataContext.Machines,
                    j => j.Aisle.Id,
                    m => m.AisleId,
                    (j, m) => new
                    {
                        Item = j.Item,
                        Machine = m,
                        Quantity = j.Quantity,
                    })
                .GroupBy(x => x.Item)
                .Select(g => new Item
                {
                    Id = g.Key.Id,
                    Description = g.Key.Description,
                    Machines = g.GroupBy(x => x.Machine)
                        .Select(
                            g2 => new MachineWithdraw
                            {
                                Id = g2.Key.Id,
                                Nickname = g2.Key.Nickname,
                                AvailableQuantityItem = g2.Sum(x => x.Quantity),
                            }).Distinct(),
                });
        }

        #endregion
    }
}
