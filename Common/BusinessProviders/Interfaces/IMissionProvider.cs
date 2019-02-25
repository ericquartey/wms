using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMissionProvider : IPagedBusinessProvider<Mission, int>,
        IReadSingleAsyncProvider<Mission, int>
    {
    }
}
