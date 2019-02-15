using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemProvider :
        IPagedBusinessProvider<Item>,
        ICreateAsyncProvider<ItemDetails>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails>,
        IDeleteAsyncProvider
    {
        #region Methods

        Task<IEnumerable<AllowedItemInCompartment>> GetAllowedByCompartmentIdAsync(int compartmentId);

        Task<ItemDetails> GetNewAsync();

        Task<IOperationResult> WithdrawAsync(ItemWithdraw itemWithdraw);

        #endregion
    }
}
