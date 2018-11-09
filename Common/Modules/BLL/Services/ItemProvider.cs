using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Item, bool>> AClassFilter =
            item => item.AbcClassId == "A";

        private static readonly Expression<Func<DataModels.Item, bool>> FifoFilter =
            item => item.ItemManagementType != null && item.ItemManagementType.Description.Contains("FIFO");

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public ItemProvider(
            DatabaseContext dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Item> GetAll()
        {
            lock (this.dataContext)
            {
                return GetAllItemsWithAggregations(this.dataContext);
            }
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Items.AsNoTracking().Count();
            }
        }

        public IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Compartments
                .Where(c => c.Id == compartmentId)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .SelectMany(
                    c => c.CompartmentType.ItemsCompartmentTypes,
                    (c, ict) => new AllowedItemInCompartment
                    {
                        Id = ict.Item.Id,
                        Code = ict.Item.Code,
                        Description = ict.Item.Description,
                        MaxCapacity = ict.MaxCapacity,
                    }
                )
                .AsNoTracking();
            }
        }

        public ItemDetails GetById(int id)
        {
            lock (this.dataContext)
            {
                var itemDetails = this.dataContext.Items
                .Include(i => i.MeasureUnit)
                .Include(i => i.ItemManagementType)
                .Where(i => i.Id == id)
                .GroupJoin(
                    this.dataContext.Compartments
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
                        ItemManagementTypeId = a.Item.ItemManagementTypeId,
                        ItemManagementTypeDescription = a.Item.ItemManagementType.Description,
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
                .Single();

                itemDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
                itemDetails.MeasureUnitChoices = this.enumerationProvider.GetAllMeasureUnits();
                itemDetails.ItemManagementTypeChoices = this.enumerationProvider.GetAllItemManagementTypes();
                itemDetails.ItemCategoryChoices = this.enumerationProvider.GetAllItemCategories();

                return itemDetails;
            }
        }

        public IQueryable<Item> GetWithAClass()
        {
            lock (this.dataContext)
            {
                return GetAllItemsWithAggregations(this.dataContext, AClassFilter);
            }
        }

        public int GetWithAClassCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Items.AsNoTracking().Count(AClassFilter);
            }
        }

        public IQueryable<Item> GetWithFifo()
        {
            lock (this.dataContext)
            {
                return GetAllItemsWithAggregations(this.dataContext, FifoFilter);
            }
        }

        public int GetWithFifoCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Items
                    .AsNoTracking()
                    .Include(i => i.ItemManagementType)
                    .Count(FifoFilter);
            }
        }

        public bool HasAnyCompartments(int itemId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Compartments.AsNoTracking().Any(c => c.ItemId == itemId);
            }
        }

        public int Save(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Items.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
                existingModel.LastModificationDate = DateTime.Now;

                return this.dataContext.SaveChanges();
            }
        }

        public async Task WithdrawAsync(ItemWithdraw itemWithdraw)
        {
            var itemsClient = ServiceLocator.Current.GetInstance<WMS.Scheduler.WebAPI.Contracts.IItemsClient>();

            await itemsClient.WithdrawAsync(
                new WMS.Scheduler.WebAPI.Contracts.WithdrawRequest
                {
                    ItemId = itemWithdraw.ItemDetails.Id,
                    BayId = itemWithdraw.BayId,
                    Lot = itemWithdraw.Lot,
                    Quantity = itemWithdraw.Quantity,
                    RegistrationNumber = itemWithdraw.RegistrationNumber,
                    Sub1 = itemWithdraw.Sub1,
                    Sub2 = itemWithdraw.Sub2
                }
            );
        }

        private static IQueryable<Item> GetAllItemsWithAggregations(DatabaseContext context, Expression<Func<DataModels.Item, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);
            lock (context)
            {
                return context.Items
               .AsNoTracking()
               .Include(i => i.AbcClass)
               .Include(i => i.ItemManagementType)
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
                       InventoryDate = a.Item.InventoryDate,
                       InventoryTolerance = a.Item.InventoryTolerance,
                       ItemManagementTypeDescription = a.Item.ItemManagementType.Description,
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
        }

        #endregion Methods
    }
}
