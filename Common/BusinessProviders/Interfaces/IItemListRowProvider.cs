using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediately(int listId, int areaId, int bayId);

        IQueryable<ItemListRow> GetByItemListId(Int32 id);

        Task<OperationResult> ScheduleForExecution(int listId, int areaId);

        #endregion Methods
    }
}
