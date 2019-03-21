using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellPositionProvider : IReadAllAsyncProvider<Enumeration, int>
    {
    }
}
