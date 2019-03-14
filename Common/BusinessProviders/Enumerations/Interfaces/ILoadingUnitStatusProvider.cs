using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitStatusProvider : IReadAllAsyncProvider<EnumerationString, string>,
        IReadSingleAsyncProvider<EnumerationString, string>
    {
    }
}
