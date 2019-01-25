using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Item, bool>> AClassFilter =
            item => item.AbcClassId == "A";

        private static readonly Expression<Func<DataModels.Item, bool>> FifoFilter =
            item => item.ManagementType == DataModels.ItemManagementType.FIFO;

        private readonly IDatabaseContextService dataContext;

        private readonly EnumerationProvider enumerationProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly WMS.Scheduler.WebAPI.Contracts.IItemsService itemsService;

        #endregion Fields

        #region Constructors

        public ItemProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Scheduler.WebAPI.Contracts.IItemsService itemsService)
        {
            this.dataContext = dataContext;
            this.itemsService = itemsService;
            this.itemsDataService = itemsDataService;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

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
            string where = null,
            IEnumerable<SortOption> orderBy = null,
            string search = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.itemsDataService.GetAllAsync(skip, take, where, orderByString, search))
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

        public IQueryable<Item> GetWithAClass()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current, AClassFilter);
        }

        public int GetWithAClassCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Items.AsNoTracking().Count(AClassFilter);
            }
        }

        public IQueryable<Item> GetWithFifo()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current, FifoFilter);
        }

        public int GetWithFifoCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Items
                    .AsNoTracking()
                    .Count(FifoFilter);
            }
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
                await this.itemsDataService.UpdateAsync(model);

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
                await this.itemsService.WithdrawAsync(
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

        private static IQueryable<Item> GetAllItemsWithAggregations(DatabaseContext context, Expression<Func<DataModels.Item, bool>> whereFunc = null)
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

        #endregion Methods
    }
}
