using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider
    {
        #region Methods

        Task<OperationResult<ItemListRow>> UpdateAsync(ItemListRow model);

        #endregion
    }
}
