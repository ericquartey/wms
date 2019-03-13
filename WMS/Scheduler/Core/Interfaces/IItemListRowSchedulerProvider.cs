using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider :
        IUpdateAsyncProvider<ItemListRow, int>
    {
        #region Methods

        Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(ListRowExecutionRequest model);

        #endregion
    }
}
