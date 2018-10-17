using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface IItemProvider : IBusinessProvider<Item, ItemDetails>
    {
        #region Methods

        IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId);

        IQueryable<Item> GetWithAClass();

        int GetWithAClassCount();

        IQueryable<Item> GetWithFifo();

        int GetWithFifoCount();

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
