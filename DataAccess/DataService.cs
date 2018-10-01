using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.DataModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.DataAccess
{
    public class DataService : IDataService
    {
        #region Fields

        private static readonly Func<DataModels.Item, bool> AClassFilter =
            item => item.AbcClassId == "A";

        private static readonly Func<DataModels.Item, bool> FifoFilter =
            item => item.ItemManagementType.Description.Contains("FIFO");

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public DataService(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<object> GetAllAbcClasses()
        {
            return this.dataContext.AbcClasses;
        }

        public IEnumerable<object> GetAllCompartments()
        {
            return this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .Include(c => c.MaterialStatus)
                .Include(c => c.Item)
                .Include(c => c.CompartmentType)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.PackageType)
                .Select(c => new
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
                );
        }

        public int GetAllCompartmentsCount()
        {
            return this.dataContext.Compartments.Count();
        }

        public IEnumerable<object> GetAllCompartmentStatuses()
        {
            return this.dataContext.CompartmentStatuses;
        }

        public IEnumerable<object> GetAllCompartmentTypes()
        {
            return this.dataContext.CompartmentTypes;
        }

        public IEnumerable<object> GetAllItemManagementTypes()
        {
            return this.dataContext.ItemManagementTypes;
        }

        public IEnumerable<object> GetAllItems()
        {
            return this.GetAllItemsWithAggregations().ToList();
        }

        public int GetAllItemsCount()
        {
            return this.dataContext.Items.Count();
        }

        public IEnumerable<object> GetAllLoadingUnits()
        {
            var aaa = this.dataContext.LoadingUnits
                .Include(l => l.LoadingUnitType)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Select(l => new
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
                    CellSide = l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                }
                );

            return aaa;
        }

        public int GetAllLoadingUnitsCount()
        {
            return this.dataContext.LoadingUnits.Count();
        }

        public IEnumerable<object> GetAllMaterialStatuses()
        {
            return this.dataContext.MaterialStatuses;
        }

        public IEnumerable<object> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits;
        }

        public IEnumerable<object> GetAllPackageTypes()
        {
            return this.dataContext.PackageTypes;
        }

        public object GetCompartmentDetails(int compartmentId)
        {
            return this.dataContext.Compartments
                .Where(compartment => compartment.Id == compartmentId)
                .Include(compartment => compartment.LoadingUnit)
                .Include(compartment => compartment.Item)
                .Single();
        }

        public IEnumerable<object> GetCompartmentsByItemId(int itemId)
        {
            return this.dataContext.Compartments
                .Where(c => c.ItemId == itemId)
                .Include(c => c.LoadingUnit)
                .Include(c => c.CompartmentStatus)
                .Select(c => ProjectCompartment(c))
                .ToList();
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(i => i.Id == itemId);
        }

        public IEnumerable<object> GetItemsWithAClass()
        {
            return this.GetAllItemsWithAggregations(AClassFilter).ToList();
        }

        public int GetItemsWithAClassCount()
        {
            return this.dataContext.Items.Count(AClassFilter);
        }

        public IEnumerable<object> GetItemsWithFifo()
        {
            return this.GetAllItemsWithAggregations(FifoFilter).ToList();
        }

        public int GetItemsWithFifoCount()
        {
            return this.dataContext.Items.Include(i => i.ItemManagementType).Count(FifoFilter);
        }

        public void Initialize()
        {
            this.dataContext.Items.ToArray();
        }

        public int SaveCompartment(IEntity<int> compartmentDetails)
        {
            return this.dataContext.SaveChanges();
        }

        public int SaveItem(IEntity<int> itemDetails)
        {
            return this.dataContext.SaveChanges();
        }

        public int SaveLoadingUnit(IEntity<int> loadingUnit)
        {
            var loadingUnitToUpdate = this.dataContext.LoadingUnits.Single(l => l.Id == loadingUnit.Id);

            return this.dataContext.SaveChanges();
        }

        private static object ProjectCompartment(DataModels.Compartment compartment)
        {
            return new
            {
                Code = compartment.Code,
                CompartmentStatusDescription = compartment.CompartmentStatus?.Description,
                CompartmentTypeDescription = compartment.CompartmentType?.Description,
                Id = compartment.Id,
                ItemDescription = compartment.Item?.Description,
                LoadingUnitCode = compartment.LoadingUnit?.Code,
                Lot = compartment.Lot,
                MaterialStatusDescription = compartment.MaterialStatus?.Description,
                Stock = compartment.Stock,
                Sub1 = compartment.Sub1,
                Sub2 = compartment.Sub2,
            };
        }

        private IQueryable<object> GetAllItemsWithAggregations(Func<DataModels.Item, bool> whereCondition = null)
        {
            return this.dataContext.Items
                .Where(i => whereCondition == null || whereCondition(i))
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
                    (a, b) => new ItemDTO
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
