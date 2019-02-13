using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemListRowSchedulerProvider
    {
        #region Methods

        Task<IOperationResult<ItemListRow>> UpdateAsync(ItemListRow model);

        #endregion
    }
}
