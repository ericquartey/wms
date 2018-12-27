using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        IQueryable<ItemListRow> GetByItemListId(int id);

        #endregion Methods
    }
}
