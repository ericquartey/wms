using System.Collections.Generic;

namespace Ferretto.Common.DataAccess
{
    public interface IDataService
    {
        #region Methods

        IEnumerable<object> GetAllClassAItems();

        IEnumerable<object> GetAllItems();

        object GetItemDetails(int itemId);

        void Initialize();

        #endregion Methods
    }
}
