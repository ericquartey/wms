using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICellPositionProvider :
        IReadAllAsyncProvider<CellPosition, int>,
        IReadSingleAsyncProvider<CellPosition, int>
    {
    }
}
