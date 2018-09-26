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
            return 0;
        }

        public object GetCompartmentsByItemId(int itemId)
        {
            return null;
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(item => item.Id == itemId);
        }

        public IEnumerable<object> GetItemsWithAClass()
        {
            return null;
        }

        public int GetItemsWithAClassCount()
        {
            return 0;
        }

        public IEnumerable<object> GetItemsWithFifo()
        {
            return null;
        }

        public int GetItemsWithFifoCount()
        {
            return 0;
        }

        public void Initialize()
        {
            this.dataContext.Items.ToArray();
        }

        #endregion Methods
    }
}
