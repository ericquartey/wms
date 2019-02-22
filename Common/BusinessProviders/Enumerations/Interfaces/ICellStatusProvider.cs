using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellStatusProvider : IReadAllAsyncProvider<Enumeration, int>
    {
    }
}
