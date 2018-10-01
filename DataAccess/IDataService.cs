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

        IEnumerable<object> GetAllCompartmentStatuses();

        IEnumerable<object> GetAllCompartmentTypes();

        IEnumerable<object> GetAllItemCategories();

        IEnumerable<object> GetAllItemManagementTypes();

        IEnumerable<object> GetAllItems();

        int GetAllItemsCount();

        IEnumerable<object> GetAllLoadingUnits();

        int GetAllLoadingUnitsCount();

        IEnumerable<object> GetAllMaterialStatuses();

        IEnumerable<object> GetAllMeasureUnits();

        IEnumerable<object> GetAllPackageTypes();

        object GetCompartmentDetails(int compartmentId);

        IEnumerable<object> GetCompartmentsByItemId(int itemId);

        object GetItemDetails(int itemId);

        IEnumerable<object> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<object> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        void Initialize();

        int SaveCompartment(IEntity<int> compartment);

        int SaveItem(IEntity<int> item);

        int SaveLoadingUnit(IEntity<int> loadingUnit);

        #endregion Methods
    }
}
