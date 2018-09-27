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

        public IEnumerable<System.Object> GetAllItemManagementTypes()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<object> GetAllItems()
        {
            return this.dataContext.Items
                .Include(item => item.AbcClass)
                .Include(item => item.Compartments)
                .Select(item =>
                new
                {
                    AbcClassDescription = item.AbcClass.Description,
                    AverageWeight = item.AverageWeight,
                    AbcClassId = item.AbcClassId,
                    CreationDate = item.CreationDate,
                    FifoTimePick = item.FifoTimePick,
                    FifoTimeStore = item.FifoTimeStore,
                    Height = item.Height,
                    Id = item.Id,
                    InventoryDate = item.InventoryDate,
                    InventoryTolerance = item.InventoryTolerance,
                    ItemManagementTypeDescription = item.ItemManagementType.Description,
                    LastModificationDate = item.LastModificationDate,
                    LastPickDate = item.LastPickDate,
                    LastStoreDate = item.LastStoreDate,
                    Length = item.Length,
                    MeasureUnitDescription = item.MeasureUnit.Description,
                    PickTolerance = item.PickTolerance,
                    ReorderPoint = item.ReorderPoint,
                    ReorderQuantity = item.ReorderQuantity,
                    StoreTolerance = item.StoreTolerance,
                    Width = item.Width,
                    Code = item.Code,
                    Description = item.Description,
                    Compartments = item.Compartments.Select(c => new { c.ReservedForPick, c.ReservedToStore, c.Stock })
                }
                )
                .ToList();
        }

        public int GetAllItemsCount()
        {
            return this.GetAllItems().Count();
        }

        public IEnumerable<object> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits;
        }

        public IEnumerable<object> GetCompartmentsByItemId(int itemId)
        {
            return this.dataContext.Compartments
                .Where(compartment => compartment.ItemId == itemId)
                .ToList();
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(item => item.Id == itemId);
        }

        public IEnumerable<object> GetItemsWithAClass()
        {
            return this.dataContext.Items.Where(item => item.AbcClassId == "A");
        }

        public int GetItemsWithAClassCount()
        {
            return this.GetItemsWithAClass().Count();
        }

        public IEnumerable<object> GetItemsWithFifo()
        {
            return this.dataContext.Items
                .Where(item => item.ItemManagementType.Description.Contains("FIFO"))
                .ToList();
        }

        public int GetItemsWithFifoCount()
        {
            return this.GetItemsWithFifo().Count();
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

        #endregion Methods
    }
}
