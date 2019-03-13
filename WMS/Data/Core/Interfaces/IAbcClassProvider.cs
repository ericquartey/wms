using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAbcClassProvider :
        IReadAllAsyncProvider<AbcClass, string>,
        IReadSingleAsyncProvider<AbcClass, string>
    {
    }
}
