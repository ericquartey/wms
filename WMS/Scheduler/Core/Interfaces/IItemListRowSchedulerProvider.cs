using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider
    {
        #region Methods

        Task<IOperationResult<ItemListRow>> PrepareForExecutionAsync(ListRowExecutionRequest model);

        Task<IOperationResult<ItemListRow>> UpdateAsync(ItemListRow model);

        #endregion
    }
}
