using System;
using System.Collections.Generic;
using System.Linq;
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
                .Include(compartment => compartment.LoadingUnit)
                .Include(compartment => compartment.MaterialStatus)
                .Include(compartment => compartment.Item)
                .Include(compartment => compartment.CompartmentType)
                .Include(compartment => compartment.CompartmentStatus)
                .Include(compartment => compartment.PackageType)
                .Select(compartment => new
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
            return this.GetAllItems().Count();
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
                .Where(compartment => compartment.ItemId == itemId)
                .Include(compartment => compartment.LoadingUnit)
                .Include(compartment => compartment.CompartmentStatus)
                .Select(compartment => ProjectCompartment(compartment))
                .ToList();
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items
                .Single(item => item.Id == itemId);
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
            var compartmentToUpdate = this.dataContext.Compartments.Single(compartment => compartment.Id == compartmentDetails.Id);

            return this.dataContext.SaveChanges();
        }

        public int SaveItem(IEntity<int> itemDetails)
        {
            var compartmentToUpdate = this.dataContext.Compartments.Single(item => item.Id == itemDetails.Id);

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
