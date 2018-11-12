using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemProvider : IBusinessProvider<Item, ItemDetails>
    {
        #region Methods

        IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId);

        IQueryable<Item> GetWithAClass();

        int GetWithAClassCount();

        IQueryable<Item> GetWithFifo();

        int GetWithFifoCount();

        bool HasAnyCompartments(int itemId);

        Task WithdrawAsync(ItemWithdraw itemWithdraw);

        #endregion Methods
    }
}
