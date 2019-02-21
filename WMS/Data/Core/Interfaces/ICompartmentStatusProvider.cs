using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentStatusProvider :
        IReadAllAsyncProvider<CompartmentStatus, int>,
        IReadSingleAsyncProvider<CompartmentStatus, int>
    {
    }
}
