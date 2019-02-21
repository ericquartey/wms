using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitStatusProvider : IReadAllAsyncProvider<EnumerationString, string>,
        IReadSingleAsyncProvider<EnumerationString, string>
    {
    }
}
