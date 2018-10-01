using System;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class MasterDataProvider : IBusinessProvider
    {
        #region Fields

        private static readonly Predicate<DataModels.Item> AClassFilter =
           item => item.AbcClassId == "A";

        private static readonly Predicate<DataModels.Item> FifoFilter =
            item => item.ItemManagementType != null && item.ItemManagementType.Description.Contains("FIFO");

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public MasterDataProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public IQueryable<DataModels.AbcClass> GetAllAbcClasses()
        {
            return this.dataContext.AbcClasses.AsNoTracking();
        }

        public IQueryable<Compartment> GetAllCompartments()
        {
            return this.dataContext.Compartments
                .Include(compartment => compartment.LoadingUnit)
                .Include(compartment => compartment.MaterialStatus)
                .Include(compartment => compartment.Item)
                .Include(compartment => compartment.CompartmentType)
                .Include(compartment => compartment.CompartmentStatus)
                .Include(compartment => compartment.PackageType)
                .Select(compartment => new Compartment
                {
                    Code = compartment.Code,
                    CompartmentStatusDescription = compartment.CompartmentStatus.Description,
                    CompartmentTypeDescription = compartment.CompartmentType.Description,
                    Id = compartment.Id,
                    ItemDescription = compartment.Item.Description,
                    LoadingUnitCode = compartment.LoadingUnit.Code,
                    Lot = compartment.Lot,
                    MaterialStatusDescription = compartment.MaterialStatus.Description,
                    Stock = compartment.Stock,
                    Sub1 = compartment.Sub1,
                    Sub2 = compartment.Sub2
                }
                );
        }

        public int GetAllCompartmentsCount()
        {
            return this.GetAllCompartments().Count();
        }

        public IQueryable<DataModels.CompartmentStatus> GetAllCompartmentStatuses()
        {
            return this.dataContext.CompartmentStatuses.AsNoTracking();
        }

        public IQueryable<DataModels.CompartmentType> GetAllCompartmentTypes()
        {
            return this.dataContext.CompartmentTypes.AsNoTracking();
        }

        public IQueryable<DataModels.ItemManagementType> GetAllItemManagementTypes()
        {
            return this.dataContext.ItemManagementTypes.AsNoTracking();
        }

        public IQueryable<Item> GetAllItems()
        {
            return this.GetAllItemsWithAggregations().AsNoTracking();
        }

        public int GetAllItemsCount()
        {
            return this.dataContext.Items.Count();
        }

        public IQueryable<DataModels.MaterialStatus> GetAllMaterialStatuses()
        {
            return this.dataContext.MaterialStatuses.AsNoTracking();
        }

        public IQueryable<DataModels.MeasureUnit> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits.AsNoTracking();
        }

        public IQueryable<DataModels.PackageType> GetAllPackageTypes()
        {
            return this.dataContext.PackageTypes.AsNoTracking();
        }

        public CompartmentDetails GetCompartmentDetails(int compartmentId)
        {
            var compartmentDetails = this.dataContext.Compartments
                .Where(c => c.Id == compartmentId)
                .Include(c => c.LoadingUnit)
                .Include(c => c.Item)
                .Select(c => ProjectCompartmentDetails(c))
                .Single();

            compartmentDetails.CompartmentStatusChoices = this.GetAllCompartmentStatuses().ToList();
            compartmentDetails.CompartmentTypeChoices = this.GetAllCompartmentTypes().ToList();
            compartmentDetails.MaterialStatusChoices = this.GetAllMaterialStatuses().ToList();
            compartmentDetails.PackageTypeChoices = this.GetAllPackageTypes().ToList();

            return compartmentDetails;
        }

        public IQueryable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            return this.dataContext.Compartments
                .Where(c => c.ItemId == itemId)
                .Include(c => c.LoadingUnit)
                .Include(c => c.CompartmentStatus)
                .Select(c => ProjectCompartment(c))
                .AsNoTracking();
        }

        public ItemDetails GetItemDetails(int itemId)
        {
            var itemDetails = this.dataContext.Items // TODO: remove duplication of this logic
                .Where(i => i.Id == itemId)
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
                    (a, b) => new ItemDetails
                    {
                        AbcClassId = a.Item.AbcClassId,
                        AverageWeight = a.Item.AverageWeight,
                        CreationDate = a.Item.CreationDate,
                        FifoTimePick = a.Item.FifoTimePick,
                        FifoTimeStore = a.Item.FifoTimeStore,
                        Height = a.Item.Height,
                        Id = a.Item.Id,
                        InventoryDate = a.Item.InventoryDate,
                        InventoryTolerance = a.Item.InventoryTolerance,
                        ItemManagementTypeId = a.Item.ItemManagementTypeId,
                        Image = a.Item.Image,
                        LastModificationDate = a.Item.LastModificationDate,
                        LastPickDate = a.Item.LastPickDate,
                        LastStoreDate = a.Item.LastStoreDate,
                        Length = a.Item.Length,
                        MeasureUnitId = a.Item.MeasureUnitId,
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
                ).Single();

            itemDetails.AbcClassChoices = this.GetAllAbcClasses();
            itemDetails.MeasureUnitChoices = this.GetAllMeasureUnits();
            itemDetails.ItemManagementTypeChoices = this.GetAllItemManagementTypes();

            return itemDetails;
        }

        public IQueryable<Item> GetItemsWithAClass()
        {
            return this.GetAllItemsWithAggregations(AClassFilter).AsNoTracking();
        }

        public int GetItemsWithAClassCount()
        {
            return this.dataContext.Items.Count(i => AClassFilter(i));
        }

        public IQueryable<Item> GetItemsWithFifo()
        {
            return this.GetAllItemsWithAggregations(FifoFilter).AsNoTracking();
        }

        public int GetItemsWithFifoCount()
        {
            return this.dataContext.Items
                .Include(i => i.ItemManagementType)
                .Count(i => FifoFilter(i));
        }

        public void Initialize()
        {
            this.dataContext.Items.Load();
        }

        public int Save(CompartmentDetails compartmentDetails)
        {
            return this.dataContext.SaveChanges();
        }

        public int Save(ItemDetails itemDetails)
        {
            return this.dataContext.SaveChanges();
        }

        private static Compartment ProjectCompartment(DataModels.Compartment c) => new Compartment
        {
            Code = c.Code,
            CompartmentStatusDescription = c.CompartmentStatus?.Description,
            CompartmentTypeDescription = c.CompartmentType?.Description,
            Id = c.Id,
            ItemDescription = c.Item?.Description,
            LoadingUnitCode = c.LoadingUnit?.Code,
            Lot = c.Lot,
            MaterialStatusDescription = c.MaterialStatus?.Description,
            Stock = c.Stock,
            Sub1 = c.Sub1,
            Sub2 = c.Sub2,
        };

        private static CompartmentDetails ProjectCompartmentDetails(DataModels.Compartment c) => new CompartmentDetails
        {
            Id = c.Id,
            Code = c.Code,
            LoadingUnitCode = c.LoadingUnit?.Code,
            CompartmentTypeId = c.CompartmentTypeId,
            ItemPairing = c.ItemPairing,
            ItemCode = c.Item?.Code,
            ItemDescription = c.Item?.Description,
            Sub1 = c.Sub1,
            Sub2 = c.Sub2,
            MaterialStatusId = c.MaterialStatusId,
            FifoTime = c.FifoTime,
            PackageTypeId = c.PackageTypeId,
            Lot = c.Lot,
            RegistrationNumber = c.RegistrationNumber,
            MaxCapacity = c.MaxCapacity,
            Stock = c.Stock,
            ReservedForPick = c.ReservedForPick,
            ReservedToStore = c.ReservedToStore,
            CompartmentStatusId = c.CompartmentStatusId,
            CreationDate = c.CreationDate,
            LastHandlingDate = c.LastHandlingDate,
            InventoryDate = c.InventoryDate,
            FirstStoreDate = c.FirstStoreDate,
            LastStoreDate = c.LastStoreDate,
            LastPickDate = c.LastPickDate,
            Width = c.Width,
            Height = c.Height,
            XPosition = c.XPosition,
            YPosition = c.YPosition
        };

        private IQueryable<Item> GetAllItemsWithAggregations(Predicate<DataModels.Item> wherePredicate = null)
        {
            return this.dataContext.Items
               .Where(i => wherePredicate == null || wherePredicate(i))
               .Include(i => i.AbcClass)
               .Include(i => i.ItemManagementType)
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
