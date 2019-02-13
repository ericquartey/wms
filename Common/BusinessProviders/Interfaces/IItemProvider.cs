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

        IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId);

        Task<ItemDetails> GetNewAsync();

        bool HasAnyCompartments(int itemId);

        Task<IOperationResult> WithdrawAsync(ItemWithdraw itemWithdraw);

        #endregion
    }
}
