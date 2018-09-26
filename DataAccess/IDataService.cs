using System.Collections.Generic;

namespace Ferretto.Common.DataAccess
{
    public interface IDataService
    {
        #region Methods

        IEnumerable<object> GetAllCompartments();

        int GetAllCompartmentsCount();

        IEnumerable<object> GetAllItems();

        int GetAllItemsCount();

        object GetCompartmentsByItemId(int itemId);

        object GetItemDetails(int itemId);

        IEnumerable<object> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<object> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        void Initialize();

        #endregion Methods
    }
}
