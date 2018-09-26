using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.EF;

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

        public IEnumerable<object> GetAllClassAItems()
        {
            return this.GetAllItems().Cast<DataModels.Item>().Where(item => item.ClassId == "A");
        }

        public IEnumerable<object> GetAllCompartments()
        {
            throw new System.NotImplementedException();
        }

        public int GetAllCompartmentsCount()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<object> GetAllItems()
        {
            return this.dataContext.Items;
        }

        public int GetAllItemsCount()
        {
            throw new System.NotImplementedException();
        }

        public object GetCompartmentsByItemId(int itemId)
        {
            throw new System.NotImplementedException();
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(item => item.Id == itemId);
        }

        public IEnumerable<object> GetItemsWithAClass()
        {
            throw new System.NotImplementedException();
        }

        public int GetItemsWithAClassCount()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<object> GetItemsWithFifo()
        {
            throw new System.NotImplementedException();
        }

        public int GetItemsWithFifoCount()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            this.dataContext.Items.ToArray();
        }

        #endregion Methods
    }
}
