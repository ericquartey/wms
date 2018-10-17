using System;
using System.Linq;
using System.Linq.Expressions;
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

        public ItemProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;

            //TODO: use interface for EnumerationProvider
            this.enumerationProvider = new EnumerationProvider(dataContext);
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Item> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllItemsWithAggregations(context);
        }

        public int GetAllCount()
        {
            return this.dataContext.Items.AsNoTracking().Count();
        }

        public ItemDetails GetById(int id)
        {
            var itemDetails = this.dataContext.Items
                .Where(i => i.Id == id)
                .Select(i => new ItemDetails
                {
                    Id = i.Id,
                    Code = i.Code,
                    Description = i.Description,
                    ItemCategoryId = i.ItemCategoryId,
                    Note = i.Note,

                    AbcClassId = i.AbcClassId,
                    MeasureUnitId = i.MeasureUnitId,
                    ItemManagementTypeId = i.ItemManagementTypeId,
                    FifoTimePick = i.FifoTimePick,
                    FifoTimeStore = i.FifoTimeStore,
                    ReorderPoint = i.ReorderPoint,
                    ReorderQuantity = i.ReorderQuantity,

                    Height = i.Height,
                    Length = i.Length,
                    Width = i.Width,
                    PickTolerance = i.PickTolerance,
                    StoreTolerance = i.StoreTolerance,
                    InventoryTolerance = i.InventoryTolerance,
                    AverageWeight = i.AverageWeight,

                    Image = i.Image,

                    CreationDate = i.CreationDate,
                    InventoryDate = i.InventoryDate,
                    LastModificationDate = i.LastModificationDate,
                    LastPickDate = i.LastPickDate,
                    LastStoreDate = i.LastStoreDate,
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

        public IQueryable<Item> GetWithAClass()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllItemsWithAggregations(context, AClassFilter);
        }

        public int GetWithAClassCount()
        {
            return this.dataContext.Items.AsNoTracking().Count(AClassFilter);
        }

        public IQueryable<Item> GetWithFifo()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return GetAllItemsWithAggregations(context, FifoFilter);
        }

        public int GetWithFifoCount()
        {
            return this.dataContext.Items
                .AsNoTracking()
                .Include(i => i.ItemManagementType)
                .Count(FifoFilter);
        }

        public bool HasAnyCompartments(int itemId)
        {
            return this.dataContext.Compartments.AsNoTracking().Any(c => c.ItemId == itemId);
        }

        public int Save(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Items.Find(model.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            existingModel.LastModificationDate = DateTime.Now;

            return this.dataContext.SaveChanges();
        }

        private static IQueryable<Item> GetAllItemsWithAggregations(DatabaseContext context, Expression<Func<DataModels.Item, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Items
               .AsNoTracking()
               .Include(i => i.AbcClass)
               .Include(i => i.ItemManagementType)
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
