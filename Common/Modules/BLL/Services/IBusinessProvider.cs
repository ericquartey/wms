using System.Collections.Generic;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
    public interface IBusinessProvider
    {
        IEnumerable<Item> GetAllClassAItems();

        IEnumerable<Item> GetAllItems();

        int GetAllItemsCount();

        IEnumerable<Item> GetAllItemsFIFO();

        ItemDetails GetItemDetails(int itemId);

        int Save(ItemDetails item);

        int Save(CompartmentDetails compartment);

        IEnumerable<Compartment> GetAllCompartments();

        IEnumerable<Compartment> GetCompartmentsByItemId(int itemId);

        IEnumerable<Item> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IEnumerable<Item> GetItemsWithFIFO();

        int GetItemsWithFIFOCount();

        int GetAllCompartmentsCount();
    }
}
