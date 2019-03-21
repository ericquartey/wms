using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellTypeProvider : IReadAllAsyncProvider<Enumeration, int>
    {
    }
}
