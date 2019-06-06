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
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class ItemProvider : BaseProvider, IItemProvider
    {
        #region Fields

        private readonly IImageProvider imageProvider;

        #endregion

        #region Constructors

        public ItemProvider(DatabaseContext dataContext, IImageProvider imageProvider, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.imageProvider = imageProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.DataContext.Items.AddAsync(new Common.DataModels.Item
            {
                AbcClassId = model.AbcClassId,
                AverageWeight = model.AverageWeight,
                Code = model.Code,
                Description = model.Description,
                FifoTimePick = model.FifoTimePick,
                FifoTimePut = model.FifoTimePut,
                Height = model.Height,
                Image = model.Image,
                InventoryDate = model.InventoryDate,
                InventoryTolerance = model.InventoryTolerance,
                ItemCategoryId = model.ItemCategoryId,
                LastPickDate = model.LastPickDate,
                LastPutDate = model.LastPutDate,
                Length = model.Length,
                ManagementType = (Common.DataModels.ItemManagementType)model.ManagementType,
                MeasureUnitId = model.MeasureUnitId,
                Note = model.Note,
                PickTolerance = model.PickTolerance,
                ReorderPoint = model.ReorderPoint,
                ReorderQuantity = model.ReorderQuantity,
                PutTolerance = model.PutTolerance,
                Width = model.Width
            });

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    model.Id = entry.Entity.Id;
                    model.CreationDate = entry.Entity.CreationDate;
                    model.LastModificationDate = entry.Entity.LastModificationDate;

                    var result = await this.SaveImageAsync(model, this.DataContext.Items, this.DataContext);
                    if (!result.Success)
                    {
                        return result;
                    }
                }

                scope.Complete();
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
                SetPolicies(model);
            }

            return model;
        }

        public async Task<ItemAvailable> GetByIdForExecutionAsync(int id)
        {
            return await this.DataContext.Items
                .Select(i => new ItemAvailable
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
                this.DataContext.Items,
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
                    this.DataContext.Items,
                    this.DataContext);

                if (!result.Success)
                {
                    return result;
                }

                result = await this.SaveImageAsync(model, this.DataContext.Items, this.DataContext);

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
                var existingModel = this.DataContext.Items.Find(model.Id);
                if (existingModel == null)
                {
                    return new NotFoundOperationResult<ItemDetails>();
                }

                var areaCount =
                    await this.DataContext.ItemsAreas
                        .CountAsync(c => c.ItemId == model.Id);

                var compartmentTypeCount =
                    await this.DataContext.ItemsAreas
                        .CountAsync(c => c.ItemId == model.Id);

                if (areaCount > 0)
                {
                    var area = await this.DataContext.ItemsAreas
                        .Where(a => a.ItemId == model.Id)
                        .ToListAsync();
                    this.DataContext.RemoveRange(area);
                }

                if (compartmentTypeCount > 0)
                {
                    var compartmentType = await this.DataContext.ItemsCompartmentTypes
                        .Where(t => t.ItemId == model.Id)
                        .ToListAsync();
                    this.DataContext.RemoveRange(compartmentType);
                }

                this.DataContext.Remove(existingModel);
                await this.DataContext.SaveChangesAsync();
                scope.Complete();

            return new SuccessOperationResult<ItemDetails>(model);
        }

        private IQueryable<Item> GetAllAllowedByLoadingUnitId(int loadingUnitId)
        {
            return this.DataContext.LoadingUnits
                .Where(l => l.Id == loadingUnitId)
                .Join(
                    this.DataContext.LoadingUnitTypesAisles,
                    l => l.LoadingUnitTypeId,
                    luta => luta.LoadingUnitTypeId,
                    (l, luta) => luta)
                .Join(
                    this.DataContext.Aisles,
                    luta => luta.AisleId,
                    a => a.Id,
                    (luta, a) => a)
                .Distinct()
                .Join(
                    this.DataContext.ItemsAreas,
                    a => a.AreaId,
                    ia => ia.AreaId,
                    (a, ia) => ia)
                .Join(
                    this.DataContext.Items,
                    ia => ia.ItemId,
                    i => i.Id,
                    (ia, i) => i)
                .GroupJoin(
                    this.DataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToPut = j.Sum(x => x.ReservedToPut)
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
                        FifoTimePut = i.Item.FifoTimePut,
                        Height = i.Item.Height,
                        Image = i.Item.Image,
                        InventoryDate = i.Item.InventoryDate,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        ManagementType = (ItemManagementType)i.Item.ManagementType,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastPutDate = i.Item.LastPutDate,
                        Length = i.Item.Length,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        PickTolerance = i.Item.PickTolerance,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,
                        PutTolerance = i.Item.PutTolerance,
                        Width = i.Item.Width,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        TotalStock = c != null ? c.TotalStock : 0,
                        TotalReservedForPick = c != null ? c.TotalReservedForPick : 0,
                        TotalReservedToPut = c != null ? c.TotalReservedToPut : 0,
                        ItemCategoryId = i.Item.ItemCategoryId,
                        ItemCategoryDescription = i.Item.ItemCategory.Description,
                        AbcClassDescription = i.Item.AbcClass.Description,

                        TotalAvailable =
                            c != null
                                ? c.TotalStock + c.TotalReservedToPut - c.TotalReservedForPick
                                : 0,

                        HasAssociatedAreas = i.Item.ItemAreas.Any(),
                        CompartmentsCount = i.Item.Compartments.Count(),
                        MissionsCount = i.Item.Missions.Count(),
                        SchedulerRequestsCount = i.Item.SchedulerRequests.Count(),
                        ItemListRowsCount = i.Item.ItemListRows.Count(),
                        HasCompartmentTypes = i.Item.ItemsCompartmentTypes.Any(),
                    })
                .Distinct();
        }

        private IQueryable<Item> GetAllBase(
                    Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.DataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.DataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToPut = j.Sum(x => x.ReservedToPut)
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
                        FifoTimePut = i.Item.FifoTimePut,
                        Height = i.Item.Height,
                        Image = i.Item.Image,
                        InventoryDate = i.Item.InventoryDate,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        ManagementType = (ItemManagementType)i.Item.ManagementType,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastPutDate = i.Item.LastPutDate,
                        Length = i.Item.Length,
                        MeasureUnitDescription = i.Item.MeasureUnit.Description,
                        PickTolerance = i.Item.PickTolerance,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,
                        PutTolerance = i.Item.PutTolerance,
                        Width = i.Item.Width,
                        Code = i.Item.Code,
                        Description = i.Item.Description,
                        TotalStock = c != null ? c.TotalStock : 0,
                        TotalReservedForPick = c != null ? c.TotalReservedForPick : 0,
                        TotalReservedToPut = c != null ? c.TotalReservedToPut : 0,
                        ItemCategoryId = i.Item.ItemCategoryId,
                        ItemCategoryDescription = i.Item.ItemCategory.Description,
                        AbcClassDescription = i.Item.AbcClass.Description,

                        TotalAvailable =
                            c != null
                                ? c.TotalStock + c.TotalReservedToPut - c.TotalReservedForPick
                                : 0,

                        HasAssociatedAreas = i.Item.ItemAreas.Any(),
                        CompartmentsCount = i.Item.Compartments.Count(),
                        MissionsCount = i.Item.Missions.Count(),
                        SchedulerRequestsCount = i.Item.SchedulerRequests.Count(),
                        ItemListRowsCount = i.Item.ItemListRows.Count(),
                        HasCompartmentTypes = i.Item.ItemsCompartmentTypes.Any(),
                    });
        }

        private IQueryable<ItemDetails> GetAllDetailsBase(
            Expression<Func<Common.DataModels.Item, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Item, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.DataContext.Items
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                .GroupJoin(
                    this.DataContext.Compartments
                        .Where(c => c.ItemId != null)
                        .GroupBy(c => c.ItemId)
                        .Select(j => new
                        {
                            ItemId = j.Key,
                            TotalStock = j.Sum(x => x.Stock),
                            TotalReservedForPick = j.Sum(x => x.ReservedForPick),
                            TotalReservedToPut = j.Sum(x => x.ReservedToPut)
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
                        FifoTimePut = i.Item.FifoTimePut,
                        ReorderPoint = i.Item.ReorderPoint,
                        ReorderQuantity = i.Item.ReorderQuantity,

                        Height = i.Item.Height,
                        Length = i.Item.Length,
                        Width = i.Item.Width,
                        PickTolerance = i.Item.PickTolerance,
                        PutTolerance = i.Item.PutTolerance,
                        InventoryTolerance = i.Item.InventoryTolerance,
                        AverageWeight = i.Item.AverageWeight,

                        Image = i.Item.Image,

                        CreationDate = i.Item.CreationDate,
                        InventoryDate = i.Item.InventoryDate,
                        LastModificationDate = i.Item.LastModificationDate,
                        LastPickDate = i.Item.LastPickDate,
                        LastPutDate = i.Item.LastPutDate,

                        TotalAvailable =
                            c != null
                                ? c.TotalStock + c.TotalReservedToPut - c.TotalReservedForPick
                                : 0,

                        CompartmentsCount = i.Item.Compartments.Count(),
                        MissionsCount = i.Item.Missions.Count(),
                        HasAssociatedAreas = i.Item.ItemAreas.Any(),
                        SchedulerRequestsCount = i.Item.SchedulerRequests.Count(),
                        ItemListRowsCount = i.Item.ItemListRows.Count(),
                        HasCompartmentTypes = i.Item.ItemsCompartmentTypes.Any(),
                    });
        }

        private IQueryable<Item> GetFilteredItemByArea(int areaId)
        {
            return this.DataContext.Compartments
                .Select(c => new
                {
                    Item = c.Item,
                    Aisle = c.LoadingUnit.Cell.Aisle,
                    Quantity = c.Stock,
                })
                .Where(x => x.Aisle.AreaId == areaId)
                .Join(
                    this.DataContext.Machines,
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
                            g2 => new MachinePick
                            {
                                Id = g2.Key.Id,
                                Nickname = g2.Key.Nickname,
                                AvailableQuantityItem = g2.Sum(x => x.Quantity),
                            }).Distinct(),
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
                    return new BadRequestOperationResult<ItemDetails>(model, imageResult.Description);
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
