using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemCategoryProvider :
        IReadAllAsyncProvider<Enumeration, int>,
        IReadSingleAsyncProvider<Enumeration, int>
    {
    }
}
