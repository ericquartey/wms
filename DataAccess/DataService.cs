using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.Common.Utils;

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

        public IEnumerable<object> GetAllCompartments()
        {
            return this.dataContext.Compartments;
        }

        public int GetAllCompartmentsCount()
        {
            return this.GetAllCompartments().Count();
        }

        public IEnumerable<object> GetAllItems()
        {
            return this.dataContext.Items;
        }

        public int GetAllItemsCount()
        {
            return this.GetAllItems().Count();
        }

        public IEnumerable<object> GetCompartmentsByItemId(int itemId)
        {
            return this.dataContext.Compartments.Where(compartment => compartment.ItemId == itemId);
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(item => item.Id == itemId);
        }

        public IEnumerable<object> GetItemsWithAClass()
        {
            return this.dataContext.Items.Where(item => item.ClassId == "A");
        }

        public int GetItemsWithAClassCount()
        {
            return this.GetItemsWithAClass().Count();
        }

        public IEnumerable<object> GetItemsWithFifo()
        {
            return this.dataContext.Items.Where(item => item.ItemManagementType.Description.Contains("FIFO"));
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
