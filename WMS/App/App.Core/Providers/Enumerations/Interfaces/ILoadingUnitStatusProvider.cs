using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public interface ILoadingUnitStatusProvider : IReadAllAsyncProvider<EnumerationString, string>,
        IReadSingleAsyncProvider<EnumerationString, string>
    {
    }
}
