using System.Collections.Generic;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
    public interface IBusinessProvider
    {
        #region Methods

        IEnumerable<Item> GetAllClassAItems();

        IEnumerable<Compartment> GetAllCompartments();

        int GetAllCompartmentsCount();

        IEnumerable<Item> GetAllItems();

        int GetAllItemsCount();

        IEnumerable<Compartment> GetCompartmentsByItemId(int itemId);

        ItemDetails GetItemDetails(int itemId);

        IEnumerable<Item> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<Item> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        int Save(ItemDetails item);

        int Save(CompartmentDetails compartment);

        #endregion Methods
    }
}
