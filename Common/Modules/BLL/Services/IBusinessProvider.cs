using System.Collections.Generic;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
    public interface IBusinessProvider
    {
        #region Methods

        IEnumerable<Compartment> GetAllCompartments();

        int GetAllCompartmentsCount();

        IEnumerable<Item> GetAllItems();

        int GetAllItemsCount();

        IEnumerable<LoadingUnit> GetAllLoadingUnits();

        int GetAllLoadingUnitsCount();

        CompartmentDetails GetCompartmentDetails(int compartmentId);

        IEnumerable<Compartment> GetCompartmentsByItemId(int itemId);

        ItemDetails GetItemDetails(int itemId);

        IEnumerable<Item> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<Item> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        int Save(CompartmentDetails compartment);

        int Save(ItemDetails item);

        int Save(LoadingUnitDetails loadingUnit);

        #endregion Methods
    }
}
