using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;

namespace Ferretto.Common.Modules.BLL.Services
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

        public IEnumerable<object> GetAllItems()
        {
            return this.dataContext.Items;
        }

        public object GetItemDetails(int itemId)
        {
            return this.dataContext.Items.Single(item => item.Id == itemId);
        }

        public void Initialize()
        {
            this.dataContext.Items.ToArray();
        }

        #endregion Methods
    }
}
