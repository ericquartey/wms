using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICellTypeProvider :
        IReadAllAsyncProvider<CellType>,
        IReadSingleAsyncProvider<CellType, int>
    {
    }
}
