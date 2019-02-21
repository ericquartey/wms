using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICellTypeProvider :
        IReadAllAsyncProvider<CellType, int>,
        IReadSingleAsyncProvider<CellType, int>
    {
    }
}
