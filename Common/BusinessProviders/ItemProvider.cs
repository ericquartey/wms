using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
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

        private readonly WMS.Scheduler.WebAPI.Contracts.IItemsService itemsService;

        #endregion Fields

        #region Constructors

        public ItemProvider(
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            WMS.Scheduler.WebAPI.Contracts.IItemsService itemsService)
        {
            this.dataContext = dataContext;
            this.itemsService = itemsService;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> AddAsync(ItemDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Item> GetAll()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current);
        }

        public int GetAllCount()
        {
            using (var dataContext = this.dataContext.Current)
            {
                return dataContext.Items.AsNoTracking().Count();
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
                    }
                )
                .AsNoTracking();
        }

        public async Task<ItemDetails> GetById(int id)
        {
            var dataContext = this.dataContext.Current;

            var itemDetails = await dataContext.Items
            .Include(i => i.MeasureUnit)
            .Where(i => i.Id == id)
            .GroupJoin(
                dataContext.Compartments
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
                (a, b) => new ItemDetails
                {
                    Id = a.Item.Id,
                    Code = a.Item.Code,
                    Description = a.Item.Description,
                    ItemCategoryId = a.Item.ItemCategoryId,
                    Note = a.Item.Note,

                    AbcClassId = a.Item.AbcClassId,
                    MeasureUnitId = a.Item.MeasureUnitId,
                    MeasureUnitDescription = a.Item.MeasureUnit.Description,
                    ManagementType = (ItemManagementType)a.Item.ManagementType,
                    FifoTimePick = a.Item.FifoTimePick,
                    FifoTimeStore = a.Item.FifoTimeStore,
                    ReorderPoint = a.Item.ReorderPoint,
                    ReorderQuantity = a.Item.ReorderQuantity,

                    Height = a.Item.Height,
                    Length = a.Item.Length,
                    Width = a.Item.Width,
                    PickTolerance = a.Item.PickTolerance,
                    StoreTolerance = a.Item.StoreTolerance,
                    InventoryTolerance = a.Item.InventoryTolerance,
                    AverageWeight = a.Item.AverageWeight,

                    Image = a.Item.Image,

                    CreationDate = a.Item.CreationDate,
                    InventoryDate = a.Item.InventoryDate,
                    LastModificationDate = a.Item.LastModificationDate,
                    LastPickDate = a.Item.LastPickDate,
                    LastStoreDate = a.Item.LastStoreDate,

                    TotalAvailable = b != null
                        ? (b.TotalStock + b.TotalReservedToStore - b.TotalReservedForPick)
                        : 0,
                }
            )
            .AsNoTracking()
            .SingleAsync();

            itemDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
            itemDetails.MeasureUnitChoices = this.enumerationProvider.GetAllMeasureUnits();
            itemDetails.ManagementTypeChoices = this.enumerationProvider.GetAllItemManagementTypes();
            itemDetails.ItemCategoryChoices = this.enumerationProvider.GetAllItemCategories();

            return itemDetails;
        }

        public IQueryable<Item> GetWithAClass()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current, AClassFilter);
        }

        public int GetWithAClassCount()
        {
            using (var dataContext = this.dataContext.Current)
            {
                return dataContext.Items.AsNoTracking().Count(AClassFilter);
            }
        }

        public IQueryable<Item> GetWithFifo()
        {
            return GetAllItemsWithAggregations(this.dataContext.Current, FifoFilter);
        }

        public int GetWithFifoCount()
        {
            using (var dataContext = this.dataContext.Current)
            {
                return dataContext.Items
                    .AsNoTracking()
                    .Count(FifoFilter);
            }
        }

        public bool HasAnyCompartments(int itemId)
        {
            using (var dataContext = this.dataContext.Current)
            {
                return dataContext.Compartments.AsNoTracking().Any(c => c.ItemId == itemId);
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
                using (var dataContext = this.dataContext.Current)
                {
                    var existingModel = dataContext.Items.Find(model.Id);

                    dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                    var changedEntityCount = await dataContext.SaveChangesAsync();

                    return new OperationResult(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public async Task<OperationResult> WithdrawAsync(ItemWithdraw itemWithdraw)
        {
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
                   }
               );

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
                   }
               );
        }

        #endregion Methods
    }
}
