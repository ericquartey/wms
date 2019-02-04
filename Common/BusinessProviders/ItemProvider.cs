using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        private readonly EnumerationProvider enumerationProvider;

        private readonly IImageProvider imageProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly WMS.Scheduler.WebAPI.Contracts.IItemsSchedulerService itemsSchedulerService;

        #endregion

        #region Constructors

        public ItemProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            IImageProvider imageProvider,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Scheduler.WebAPI.Contracts.IItemsSchedulerService itemsSchedulerService)
        {
            this.dataContext = dataContext;
            this.itemsSchedulerService = itemsSchedulerService;
            this.itemsDataService = itemsDataService;
            this.enumerationProvider = enumerationProvider;
            this.imageProvider = imageProvider;
        }

        #endregion

        #region Methods

        public Task<OperationResult> AddAsync(ItemDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Item> GetAll()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current);
        }

        public async Task<IEnumerable<Item>> GetAllAsync(
            int take = 0,
            int skip = 0,
            string whereExpression = null,
            IEnumerable<SortOption> orderBy = null,
            string searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.itemsDataService.GetAllAsync(skip, take, whereExpression, orderByString, searchExpression))
                .Select(i => new Item
                {
                    Id = i.Id,
                    AbcClassDescription = i.AbcClassDescription,
                    AverageWeight = i.AverageWeight,
                    CreationDate = i.CreationDate,
                    FifoTimePick = i.FifoTimePick,
                    FifoTimeStore = i.FifoTimeStore,
                    Height = i.Height,
                    InventoryDate = i.InventoryDate,
                    InventoryTolerance = i.InventoryTolerance,
                    ManagementTypeDescription = i.ManagementType.ToString(), // TODO change
                    ItemCategoryDescription = i.ItemCategoryDescription,
                    LastModificationDate = i.LastModificationDate,
                    LastPickDate = i.LastPickDate,
                    LastStoreDate = i.LastStoreDate,
                    Length = i.Length,
                    MeasureUnitDescription = i.MeasureUnitDescription,
                    PickTolerance = i.PickTolerance,
                    ReorderPoint = i.ReorderPoint,
                    ReorderQuantity = i.ReorderQuantity,
                    StoreTolerance = i.StoreTolerance,
                    Width = i.Width,
                    Code = i.Code,
                    Description = i.Description,
                    TotalReservedForPick = i.TotalReservedForPick,
                    TotalReservedToStore = i.TotalReservedToStore,
                    TotalStock = i.TotalStock,
                    TotalAvailable = i.TotalAvailable,
                });
        }

        public int GetAllCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Items.AsNoTracking().Count();
            }
        }

        public async Task<int> GetAllCountAsync(string whereExpression = null, string searchExpression = null)
        {
            return await this.itemsDataService.GetAllCountAsync(whereExpression, searchExpression);
        }

        public IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId)
        {
            return this.dataContext.Current.Compartments
                .Where(c => c.Id == compartmentId)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.AbcClass)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.ItemCategory)
                .SelectMany(
                    c => c.CompartmentType.ItemsCompartmentTypes,
                    (c, ict) => new AllowedItemInCompartment
                    {
                        Id = ict.Item.Id,
                        Code = ict.Item.Code,
                        Description = ict.Item.Description,
                        MaxCapacity = ict.MaxCapacity,
                        AbcClassDescription = ict.Item.AbcClass.Description,
                        ItemCategoryDescription = ict.Item.ItemCategory.Description,
                        Image = ict.Item.Image,
                    })
                .AsNoTracking();
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            var item = await this.itemsDataService.GetByIdAsync(id);

            var itemDetails = new ItemDetails
            {
                Id = item.Id,
                Code = item.Code,
                Description = item.Description,
                ItemCategoryId = item.ItemCategoryId,
                Note = item.Note,

                AbcClassId = item.AbcClassId,
                MeasureUnitId = item.MeasureUnitId,

                // TODO  MeasureUnitDescription = item.MeasureUnit.,
                ManagementType = (ItemManagementType)item.ManagementType,
                FifoTimePick = item.FifoTimePick,
                FifoTimeStore = item.FifoTimeStore,
                ReorderPoint = item.ReorderPoint,
                ReorderQuantity = item.ReorderQuantity,

                Height = item.Height,
                Length = item.Length,
                Width = item.Width,
                PickTolerance = item.PickTolerance,
                StoreTolerance = item.StoreTolerance,
                InventoryTolerance = item.InventoryTolerance,
                AverageWeight = item.AverageWeight,

                Image = item.Image,

                CreationDate = item.CreationDate,
                InventoryDate = item.InventoryDate,
                LastModificationDate = item.LastModificationDate,
                LastPickDate = item.LastPickDate,
                LastStoreDate = item.LastStoreDate,

                TotalAvailable = item.TotalAvailable
            };

            itemDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
            itemDetails.MeasureUnitChoices = this.enumerationProvider.GetAllMeasureUnits();
            itemDetails.ManagementTypeChoices = EnumerationProvider.GetAllItemManagementTypes();
            itemDetails.ItemCategoryChoices = this.enumerationProvider.GetAllItemCategories();

            return itemDetails;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(
                    Resources.Errors.ParameterCannotBeNullOrWhitespace, nameof(propertyName));
            }

            return await this.itemsDataService.GetUniqueValuesAsync(propertyName);
        }

        public bool HasAnyCompartments(int itemId)
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Compartments.AsNoTracking().Any(c => c.ItemId == itemId);
            }
        }

        public async Task<OperationResult> SaveAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var originalItem = await this.itemsDataService.GetByIdAsync(model.Id);

                await this.itemsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.Item
                {
                    AbcClassId = model.AbcClassId,
                    AverageWeight = model.AverageWeight,
                    Code = model.Code,
                    Description = model.Description,
                    FifoTimePick = model.FifoTimePick,
                    FifoTimeStore = model.FifoTimeStore,
                    Height = model.Height,
                    Id = model.Id,
                    Image = model.Image,
                    InventoryDate = model.InventoryDate,
                    InventoryTolerance = model.InventoryTolerance,
                    ItemCategoryId = model.ItemCategoryId,
                    LastPickDate = model.LastPickDate,
                    LastStoreDate = model.LastStoreDate,
                    Length = model.Length,
                    ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                    MeasureUnitId = model.MeasureUnitId,
                    Note = model.Note,
                    PickTolerance = model.PickTolerance,
                    ReorderPoint = model.ReorderPoint,
                    ReorderQuantity = model.ReorderQuantity,
                    StoreTolerance = model.StoreTolerance,
                    Width = model.Width
                });

                if (originalItem.Image != model.Image)
                {
                    this.SaveImage(model.ImagePath);
                }

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public async Task<OperationResult> WithdrawAsync(ItemWithdraw itemWithdraw)
        {
            if (itemWithdraw == null)
            {
                throw new ArgumentNullException(nameof(itemWithdraw));
            }

            try
            {
                await this.itemsSchedulerService.WithdrawAsync(
                   new WMS.Scheduler.WebAPI.Contracts.SchedulerRequest
                   {
                       IsInstant = true,
                       Type = WMS.Scheduler.WebAPI.Contracts.OperationType.Withdrawal,
                       ItemId = itemWithdraw.ItemDetails.Id,
                       BayId = itemWithdraw.BayId,
                       AreaId = itemWithdraw.AreaId.Value,
                       Lot = itemWithdraw.Lot,
                       RequestedQuantity = itemWithdraw.Quantity,
                       RegistrationNumber = itemWithdraw.RegistrationNumber,
                       Sub1 = itemWithdraw.Sub1,
                       Sub2 = itemWithdraw.Sub2,
                   });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        private static IQueryable<Item> GetAllItemsWithAggregations(
            DatabaseContext context,
            Expression<Func<DataModels.Item, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Items
               .AsNoTracking()
               .Include(i => i.AbcClass)
               .Include(i => i.ItemCategory)
               .Include(i => i.MeasureUnit)
               .Where(actualWhereFunc)
               .GroupJoin(
                   context.Compartments
                       .AsNoTracking()
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
                   (a, b) => new Item
                   {
                       Id = a.Item.Id,
                       AbcClassDescription = a.Item.AbcClass.Description,
                       AverageWeight = a.Item.AverageWeight,
                       CreationDate = a.Item.CreationDate,
                       FifoTimePick = a.Item.FifoTimePick,
                       FifoTimeStore = a.Item.FifoTimeStore,
                       Height = a.Item.Height,
                       Image = a.Item.Image,
                       InventoryDate = a.Item.InventoryDate,
                       InventoryTolerance = a.Item.InventoryTolerance,
                       ManagementTypeDescription = a.Item.ManagementType.ToString(), // TODO change
                       ItemCategoryDescription = a.Item.ItemCategory.Description,
                       LastModificationDate = a.Item.LastModificationDate,
                       LastPickDate = a.Item.LastPickDate,
                       LastStoreDate = a.Item.LastStoreDate,
                       Length = a.Item.Length,
                       MeasureUnitDescription = a.Item.MeasureUnit.Description,
                       PickTolerance = a.Item.PickTolerance,
                       ReorderPoint = a.Item.ReorderPoint,
                       ReorderQuantity = a.Item.ReorderQuantity,
                       StoreTolerance = a.Item.StoreTolerance,
                       Width = a.Item.Width,
                       Code = a.Item.Code,
                       Description = a.Item.Description,
                       TotalReservedForPick = b != null ? b.TotalReservedForPick : 0,
                       TotalReservedToStore = b != null ? b.TotalReservedToStore : 0,
                       TotalStock = b != null ? b.TotalStock : 0,
                       TotalAvailable = b != null
                           ? (b.TotalStock + b.TotalReservedToStore - b.TotalReservedForPick)
                           : 0,
                   });
        }

        private void SaveImage(string imagePath)
        {
            this.imageProvider.SaveImage(imagePath);
        }

        #endregion
    }
}
