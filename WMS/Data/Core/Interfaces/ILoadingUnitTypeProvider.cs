using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ILoadingUnitTypeProvider :
        IReadAllAsyncProvider<LoadingUnitType, int>,
        IReadSingleAsyncProvider<LoadingUnitType, int>
    {
    }
}
