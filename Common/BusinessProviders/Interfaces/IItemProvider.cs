using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemProvider :
        IPagedBusinessProvider<Item, int>,
        ICreateAsyncProvider<ItemDetails, int>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails, int>,
        IDeleteAsyncProvider<ItemDetails, int>
    {
        #region Methods

        Task<IEnumerable<AllowedItemInCompartment>> GetAllowedByCompartmentIdAsync(int compartmentId);

        Task<ItemDetails> GetNewAsync();

        Task<IOperationResult<SchedulerRequest>> WithdrawAsync(ItemWithdraw itemWithdraw);

        #endregion
    }
}
