using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMachineProvider :
        IPagedBusinessProvider<Machine, int>,
        IReadSingleAsyncProvider<Machine, int>
    {
    }
}
