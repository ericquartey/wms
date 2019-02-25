using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMaterialStatusProvider : IReadAllAsyncProvider<Enumeration, int>,
        IReadSingleAsyncProvider<Enumeration, int>
    {
    }
}
