using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListProvider : IBusinessProvider<ItemList, ItemListDetails>
    {
        #region Methods

        IQueryable<ItemList> GetWithStatusCompleted();

        int GetWithStatusCompletedCount();

        IQueryable<ItemList> GetWithStatusWaiting();

        int GetWithStatusWaitingCount();

        IQueryable<ItemList> GetWithTypePick();

        int GetWithTypePickCount();

        #endregion Methods
    }
}
