using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        private readonly IImageProvider imageProvider;

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public ItemProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            IImageProvider imageProvider)
        {
            this.dataContext = dataContext;
            this.imageProvider = imageProvider;
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.Items.AddAsync(
                this.mapper.Map<Common.DataModels.Item>(model));

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    var result = await this.SaveImageAsync(model, this.dataContext.Items, this.dataContext);
                    if (!result.Success)
                    {
                        return result;
                    }
                }

                scope.Complete();
            }

            var createdItem = await this.GetByIdAsync(entry.Entity.Id);
            return new SuccessOperationResult<ItemDetails>(createdItem);
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

        public async Task<IEnumerable<Item>> GetAllAllowedByLoadingUnitIdAsync(
            int loadingUnitId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null)
        {
            var models = await this.GetAllAllowedByLoadingUnitId(loadingUnitId)
                .ToArrayAsync<Item, Common.DataModels.Item>(
                    skip,
                    take,
                    orderBySortOptions,
                    null,
                    null);

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllAllowedByLoadingUnitIdCountAsync(int loadingUnitId)
        {
            return await this.GetAllAllowedByLoadingUnitId(loadingUnitId)
                .CountAsync<Item, Common.DataModels.Item>(
                    null,
                    null);
        }

        public async Task<IEnumerable<AssociateItemWithCompartmentType>> GetAllAssociatedByCompartmentTypeIdAsync(
            int compartmentTypeId)
        {
            return await this.dataContext.ItemsCompartmentTypes
                .Where(x => x.CompartmentTypeId == compartmentTypeId)
                .Select(
                i => new
                {
                    Item = i.Item,
                    MaxCapacity = i.MaxCapacity,
                })
                .GroupJoin(
                    this.dataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToPut = j.Sum(x => x.ReservedToPut)
                        }),
                    i => i.Item.Id,
                    c => c.ItemId,
                    (i, c) => new
                    {
                        Item = i.Item,
                        MaxCapacity = i.MaxCapacity,
                        CompartmentsAggregation = c
                    })
                .SelectMany(
                    temp => temp.CompartmentsAggregation.DefaultIfEmpty(),
                    (i, c) => new AssociateItemWithCompartmentType
                    {
                        Id = i.Item.Id,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        ItemCategoryDescription = i.Item.ItemCategory.Description,
                        AbcClassDescription = i.Item.AbcClass.Description,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        MaxCapacity = i.MaxCapacity,
                        TotalStock = c.TotalStock,
                        TotalReservedForPick = c.TotalReservedForPick,
                        TotalReservedToPut = c.TotalReservedToPut,
                        TotalAvailable = c.TotalStock + c.TotalReservedToPut - c.TotalReservedForPick,
                    }).ToArrayAsync();
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
                SetPolicies(model);
            }

            return models;
        }

        public async Task<IEnumerable<Item>> GetAllByAreaIdAsync(
            int areaId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var items = await this.GetFilteredItemByArea(areaId)
                .ToArrayAsync<Item, Common.DataModels.Item>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var item in items)
            {
                SetPolicies(item);
            }

            return items;
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

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            var model = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (model != null)
            {
                SetPolicies(model);
            }

            return model;
        }

        public async Task<ItemAvailable> GetByIdForExecutionAsync(int id)
        {
            return await this.dataContext.Items
                .ProjectTo<ItemAvailable>(this.mapper.ConfigurationProvider)
                .SingleAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.dataContext.Items,
                this.GetAllBase());
        }

        public async Task<IOperationResult<ItemAvailable>> UpdateAsync(ItemAvailable model)
        {
            return await this.UpdateAsync<Common.DataModels.Item, ItemAvailable, int>(
                model,
                this.dataContext.Items,
                this.dataContext);
        }

        public async Task<IOperationResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            IOperationResult<ItemDetails> result;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                result = await this.UpdateAsync<Common.DataModels.Item, ItemDetails, int>(
                    model,
                    this.dataContext.Items,
                    this.dataContext);

                if (!result.Success)
                {
                    return result;
                }

                result = await this.SaveImageAsync(model, this.dataContext.Items, this.dataContext);

                scope.Complete();
            }

            return result;
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
                        || Equals(i.TotalReservedToPut, searchAsDouble)
                        || Equals(i.TotalStock, searchAsDouble)));
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy((model as IItemUpdatePolicy).ComputeUpdatePolicy());
            model.AddPolicy((model as IItemDeletePolicy).ComputeDeletePolicy());
            model.AddPolicy((model as IItemPickPolicy).ComputePickPolicy());
            model.AddPolicy((model as IItemPutPolicy).ComputePutPolicy());
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

        private IQueryable<Item> GetAllAllowedByLoadingUnitId(int loadingUnitId)
        {
            return this.dataContext.LoadingUnits
                .Where(l => l.Id == loadingUnitId)
                .Join(
                    this.dataContext.LoadingUnitTypesAisles,
                    l => l.LoadingUnitTypeId,
                    luta => luta.LoadingUnitTypeId,
                    (l, luta) => luta)
                .Join(
                    this.dataContext.Aisles,
                    luta => luta.AisleId,
                    a => a.Id,
                    (luta, a) => a)
                .Distinct()
                .Join(
                    this.dataContext.ItemsAreas,
                    a => a.AreaId,
                    ia => ia.AreaId,
                    (a, ia) => ia)
                .Join(
                    this.dataContext.Items,
                    ia => ia.ItemId,
                    i => i.Id,
                    (ia, i) => i)
                .Distinct()
                .ProjectTo<Item>(this.mapper.ConfigurationProvider);
        }

        private IQueryable<Item> GetAllBase(
                    Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            var items = this.dataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .ProjectTo<Item>(this.mapper.ConfigurationProvider);

            return items;
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
                .ProjectTo<ItemDetails>(this.mapper.ConfigurationProvider);
        }

        private IQueryable<Item> GetFilteredItemByArea(int areaId)
        {
            return this.dataContext.Items.Join(
                this.dataContext.Compartments
                    .Select(c => new
                    {
                        ItemId = c.ItemId,
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
                            ItemId = j.ItemId,
                            Machine = m,
                            Quantity = j.Quantity,
                        })
                    .GroupBy(x => x.ItemId),
                    i => i.Id,
                    g => g.Key,
                    (i, g) => new Item
                    {
                        AbcClassDescription = i.AbcClass.Description,
                        AbcClassId = i.AbcClassId,
                        AverageWeight = i.AverageWeight,
                        Code = i.Code,
                        CreationDate = i.CreationDate,
                        Description = i.Description,
                        FifoTimePick = i.FifoTimePick,
                        FifoTimePut = i.FifoTimePut,
                        Height = i.Height,
                        Id = i.Id,
                        Image = i.Image,
                        InventoryDate = i.InventoryDate,
                        InventoryTolerance = i.InventoryTolerance,
                        ItemCategoryDescription = i.ItemCategory.Description,
                        ItemCategoryId = i.ItemCategoryId,
                        LastModificationDate = i.LastModificationDate,
                        LastPickDate = i.LastPickDate,
                        LastPutDate = i.LastPutDate,
                        Length = i.Length,
                        ManagementType = (ItemManagementType)i.ManagementType,
                        MeasureUnitDescription = i.MeasureUnit.Description,
                        Note = i.Note,
                        PickTolerance = i.PickTolerance,
                        PutTolerance = i.PutTolerance,
                        ReorderPoint = i.ReorderPoint,
                        ReorderQuantity = i.ReorderQuantity,
                        Width = i.Width,
                        Machines = g.GroupBy(x => x.Machine)
                            .Select(
                                g2 => new MachinePick
                                {
                                    Id = g2.Key.Id,
                                    Nickname = g2.Key.Nickname,
                                    AvailableQuantityItem = g2.Sum(x => x.Quantity),
                                }).Distinct(),
                        CompartmentsCount = i.Compartments.Count(),
                        MissionsCount = i.Missions.Count(),
                        SchedulerRequestsCount = i.SchedulerRequests.Count(),
                        ItemListRowsCount = i.ItemListRows.Count(),
                        HasCompartmentTypes = i.ItemsCompartmentTypes.Any(),
                        HasAssociatedAreas = i.ItemAreas.Any(),
                        TotalStock = i.Compartments.Sum(cm => cm.Stock),
                        TotalReservedForPick = i.Compartments.Sum(cm => cm.ReservedForPick),
                        TotalReservedToPut = i.Compartments.Sum(cm => cm.ReservedToPut),
                        TotalAvailable = i.Compartments.Sum(cm => cm.Stock + cm.ReservedToPut - cm.ReservedForPick)
                    });
        }

        private async Task<IOperationResult<ItemDetails>> SaveImageAsync(
            ItemDetails model,
            DbSet<Common.DataModels.Item> dataContextItems,
            DatabaseContext databaseContext)
        {
            if (!string.IsNullOrEmpty(model.UploadImageName) && model.UploadImageData != null)
            {
                var imageResult = this.imageProvider.Create(model.UploadImageName, model.UploadImageData);
                if (!imageResult.Success)
                {
                    return new BadRequestOperationResult<ItemDetails>(imageResult.Description, model);
                }

                model.Image = imageResult.Entity;

                var result = await this.UpdateAsync<Common.DataModels.Item, ItemDetails, int>(model, dataContextItems, databaseContext);
                if (!result.Success)
                {
                    return result;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.UploadImageName) || model.UploadImageData != null)
                {
                    return new BadRequestOperationResult<ItemDetails>(model);
                }
            }

            return new SuccessOperationResult<ItemDetails>(model);
        }

        #endregion
    }
}
