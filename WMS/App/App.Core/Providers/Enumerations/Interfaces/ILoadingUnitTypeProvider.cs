using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitTypeProvider : IReadAllAsyncProvider<Enumeration, int>,
        IReadSingleAsyncProvider<Enumeration, int>
    {
    }
}
