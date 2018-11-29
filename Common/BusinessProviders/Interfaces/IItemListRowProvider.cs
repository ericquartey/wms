using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        IEnumerable<ItemListRow> GetByItemListById(Int32 id);

        #endregion Methods
    }
}
