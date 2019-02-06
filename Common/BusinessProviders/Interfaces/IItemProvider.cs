using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemProvider : IBusinessProvider<Item, ItemDetails>, IPagedBusinessProvider<Item>
    {
        #region Methods

        IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId);

        ItemDetails GetNew();

        bool HasAnyCompartments(int itemId);

        Task<OperationResult> WithdrawAsync(ItemWithdraw itemWithdraw);

        #endregion
    }
}
