using System;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private static readonly Predicate<DataModels.Item> AClassFilter =
            item => item.AbcClassId == "A";

        private static readonly Predicate<DataModels.Item> FifoFilter =
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
            return this.GetAllItemsWithAggregations();
        }

        public int GetAllCount()
        {
            return this.dataContext.Items.Count();
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
            return this.GetAllItemsWithAggregations(AClassFilter);
        }

        public int GetWithAClassCount()
        {
            return this.dataContext.Items.Count(i => AClassFilter(i));
        }

        public IQueryable<Item> GetWithFifo()
        {
            return this.GetAllItemsWithAggregations(FifoFilter).AsNoTracking();
        }

        public int GetWithFifoCount()
        {
            return this.dataContext.Items
                .Include(i => i.ItemManagementType)
                .Count(i => FifoFilter(i));
        }

        public bool HasAnyCompartments(int itemId)
        {
            return this.dataContext.Compartments.AsNoTracking().Where(c => c.ItemId == itemId).Any();
        }

        public int Save(ItemDetails model)
        {
            var item = this.dataContext.Items.Single(i => i.Id == model.Id);

            item.AbcClassId = model.AbcClassId;
            item.AverageWeight = model.AverageWeight;
            item.Code = model.Code;
            item.CreationDate = model.CreationDate;
            item.Description = model.Description;
            item.FifoTimePick = model.FifoTimePick;
            item.FifoTimeStore = model.FifoTimeStore;
            item.Height = model.Height;
            item.Image = model.Image;
            item.InventoryDate = model.InventoryDate;
            item.InventoryTolerance = model.InventoryTolerance;
            item.ItemCategoryId = model.ItemCategoryId;
            item.ItemManagementTypeId = model.ItemManagementTypeId;
            item.LastModificationDate = model.LastModificationDate;
            item.LastPickDate = model.LastPickDate;
            item.LastStoreDate = model.LastStoreDate;
            item.Length = model.Length;
            item.MeasureUnitId = model.MeasureUnitId;
            item.Note = model.Note;
            item.PickTolerance = model.PickTolerance;
            item.ReorderPoint = model.ReorderPoint;
            item.ReorderQuantity = model.ReorderQuantity;
            item.StoreTolerance = model.StoreTolerance;
            item.Width = model.Width;

            return this.dataContext.SaveChanges();
        }

        private IQueryable<Item> GetAllItemsWithAggregations(Predicate<DataModels.Item> wherePredicate = null)
        {
            return this.dataContext.Items
               .AsNoTracking()
               .Where(i => wherePredicate == null || wherePredicate(i))
               .Include(i => i.AbcClass)
               .Include(i => i.ItemManagementType)
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
                   (a, b) => new Item
                   {
                       AbcClassDescription = a.Item.AbcClass.Description,
                       AverageWeight = a.Item.AverageWeight,
                       AbcClassId = a.Item.AbcClassId,
                       CreationDate = a.Item.CreationDate,
                       FifoTimePick = a.Item.FifoTimePick,
                       FifoTimeStore = a.Item.FifoTimeStore,
                       Height = a.Item.Height,
                       Id = a.Item.Id,
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
