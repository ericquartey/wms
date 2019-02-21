using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ILoadingUnitStatusProvider :
        IReadAllAsyncProvider<LoadingUnitStatus, string>,
        IReadSingleAsyncProvider<LoadingUnitStatus, string>
    {
    }
}
