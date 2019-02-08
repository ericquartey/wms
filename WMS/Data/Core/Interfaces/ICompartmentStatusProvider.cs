using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentStatusProvider :
        IReadAllAsyncProvider<CompartmentStatus>,
        IReadSingleAsyncProvider<CompartmentStatus, int>
    {
    }
}
