using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IMachineProvider :
        IPagedBusinessProvider<Machine>,
        IReadSingleAsyncProvider<Machine, int>
    {
    }
}
