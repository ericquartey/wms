using System.Linq;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL
{
    public interface IItemProvider : IBusinessProvider<Item, ItemDetails>
    {
        #region Methods

        IQueryable<Item> GetWithAClass();

        int GetWithAClassCount();

        IQueryable<Item> GetWithFifo();

        int GetWithFifoCount();

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
