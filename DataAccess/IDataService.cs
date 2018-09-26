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

        IEnumerable<object> GetCompartmentsByItemId(int itemId);

        object GetItemDetails(int itemId);

        IEnumerable<object> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<object> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        void Initialize();

        int SaveCompartment(object compartment);

        int SaveItem(object item);

        #endregion Methods
    }
}
