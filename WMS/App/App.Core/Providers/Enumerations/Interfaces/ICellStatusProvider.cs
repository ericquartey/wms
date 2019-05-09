using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public interface ICellStatusProvider : IReadAllAsyncProvider<Enumeration, int>
    {
    }
}
