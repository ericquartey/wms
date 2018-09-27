using System.Collections.Generic;
using Ferretto.Common.Utils;

namespace Ferretto.Common.DataAccess
{
    public interface IDataService
    {
        #region Methods

        IEnumerable<object> GetAllAbcClasses();

        IEnumerable<object> GetAllCompartments();

        int GetAllCompartmentsCount();

        IEnumerable<object> GetAllItemManagementTypes();

        IEnumerable<object> GetAllItems();

        int GetAllItemsCount();

        IEnumerable<object> GetAllMeasureUnits();

        IEnumerable<object> GetCompartmentsByItemId(int itemId);

        object GetItemDetails(int itemId);

        IEnumerable<object> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<object> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        void Initialize();

        int SaveCompartment(IEntity<int> compartment);

        int SaveItem(IEntity<int> item);

        #endregion Methods
    }
}
