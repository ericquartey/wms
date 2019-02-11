using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IPackageTypeProvider :
        IReadAllAsyncProvider<PackageType, int>,
        IReadSingleAsyncProvider<PackageType, int>
    {
    }
}
