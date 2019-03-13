using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider :
        IUpdateAsyncProvider<ItemListRow, int>
    {
        #region Methods

        Task<IOperationResult<ItemListRow>> PrepareForExecutionAsync(ListRowExecutionRequest model);

        #endregion
    }
}
