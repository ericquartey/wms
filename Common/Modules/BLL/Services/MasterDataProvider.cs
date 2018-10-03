using System;
using System.Linq;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Data.Selection;
using Ferretto.Common.DataModels;
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

        public IQueryable<Enumeration<string>> GetAllAbcClasses()
        {
            return this.dataContext.AbcClasses.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllCellPositions()
        {
            return this.dataContext.CellPositions.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Compartment> GetAllCompartments()
        {
            return this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .Include(c => c.MaterialStatus)
                .Include(c => c.Item)
                .Include(c => c.CompartmentType)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.PackageType)
                .Select(c => new Compartment
                {
                    Code = c.Code,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    CompartmentTypeDescription = c.CompartmentType.Description,
                    Id = c.Id,
                    ItemDescription = c.Item.Description,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatus.Description,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2
                }
                )
                .AsNoTracking();
        }

        public int GetAllCompartmentsCount()
        {
            return this.dataContext.Compartments.Count();
        }

        public IQueryable<Enumeration<int>> GetAllCompartmentStatuses()
        {
            return this.dataContext.CompartmentStatuses.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllCompartmentTypes()
        {
            return this.dataContext.CompartmentTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllItemCategories()
        {
            return this.dataContext.ItemCategories.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllItemManagementTypes()
        {
            return this.dataContext.ItemManagementTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Item> GetAllItems()
        {
            return this.GetAllItemsWithAggregations();
        }

        public int GetAllItemsCount()
        {
            return this.dataContext.Items.Count();
        }

        public IQueryable<LoadingUnit> GetAllLoadingUnits()
        {
            return this.dataContext.LoadingUnits
                .Include(l => l.LoadingUnitType)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Select(l => new LoadingUnit
                {
                    Id = l.Id,
                    Code = l.Code,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    AbcClassDescription = l.AbcClass.Description,
                    AreaName = l.Cell.Aisle.Area.Name,
                    AisleName = l.Cell.Aisle.Name,
                    CellFloor = l.Cell.Floor,
                    CellColumn = l.Cell.Column,
                    CellSide = Enum.GetName(typeof(DataModels.Side), l.Cell.Side),
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                }
                )
                .AsNoTracking();
        }

        public Int32 GetAllLoadingUnitsCount()
        {
            return this.dataContext.LoadingUnits.Count();
        }

        public IQueryable<Enumeration<string>> GetAllLoadingUnitStatuses()
        {
            return this.dataContext.LoadingUnitStatuses.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllLoadingUnitTypes()
        {
            return this.dataContext.LoadingUnitTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllMaterialStatuses()
        {
            return this.dataContext.MaterialStatuses.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<string>> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllPackageTypes()
        {
            return this.dataContext.PackageTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
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

        public IQueryable<CompartmentDetails> GetCompartmentsByLoadingUnitId(int loadingUnitId)
        {
            return this.dataContext.Compartments
                .Where(c => c.LoadingUnitId == loadingUnitId)
                .Select(c => ProjectCompartmentDetails(c))
                .AsNoTracking();
        }

        public ItemDetails GetItemDetails(int itemId)
        {
            var itemDetails = this.dataContext.Items // TODO: remove duplication of this logic
                .Where(i => i.Id == itemId)
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

            itemDetails.AbcClassChoices = this.GetAllAbcClasses();
            itemDetails.MeasureUnitChoices = this.GetAllMeasureUnits();
            itemDetails.ItemManagementTypeChoices = this.GetAllItemManagementTypes();
            itemDetails.ItemCategoryChoices = this.GetAllItemCategories();

            return itemDetails;
        }

        public IQueryable<Item> GetItemsWithAClass()
        {
            return this.GetAllItemsWithAggregations(AClassFilter);
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

        public LoadingUnitDetails GetLoadingUnitDetails(int loadingUnitId)
        {
            var loadingUnitDetails = this.dataContext.LoadingUnits
                .Where(l => l.Id == loadingUnitId)
                .Include(l => l.LoadingUnitType)
                .ThenInclude(l => l.LoadingUnitSizeClass)
                .Select(l => ProjectLoadingUnitDetails(l))
                .Single();

            loadingUnitDetails.AbcClassChoices = this.GetAllAbcClasses();
            loadingUnitDetails.CellPositionChoices = this.GetAllCellPositions();
            loadingUnitDetails.LoadingUnitStatusChoices = this.GetAllLoadingUnitStatuses();
            loadingUnitDetails.LoadingUnitTypeChoices = this.GetAllLoadingUnitTypes();
            loadingUnitDetails.Compartments = this.GetCompartmentsByLoadingUnitId(loadingUnitId).ToList();

            return loadingUnitDetails;
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

        public int Save(LoadingUnitDetails loadingUnit)
        {
            return this.dataContext.SaveChanges();
        }

        private static Compartment ProjectCompartment(DataModels.Compartment c) =>
            new Compartment
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
                ItemPairing = c.ItemPairing.ToString(),
            };

        private static CompartmentDetails ProjectCompartmentDetails(DataModels.Compartment c) =>
            new CompartmentDetails
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

        private static LoadingUnitDetails ProjectLoadingUnitDetails(DataModels.LoadingUnit l) =>
            new LoadingUnitDetails
            {
                Id = l.Id,
                Code = l.Code,
                AbcClassId = l.AbcClassId,
                CellPositionId = l.CellPositionId,
                LoadingUnitStatusId = l.LoadingUnitStatusId,
                LoadingUnitTypeId = l.LoadingUnitTypeId,
                Width = l.LoadingUnitType.LoadingUnitSizeClass.Width,
                Length = l.LoadingUnitType.LoadingUnitSizeClass.Length,
            };

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
