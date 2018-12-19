using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        IQueryable<ItemListRow> GetByItemListId(Int32 id);

        #endregion Methods
    }
}
