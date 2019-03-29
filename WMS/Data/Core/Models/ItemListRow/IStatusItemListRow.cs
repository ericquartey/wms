using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IStatusItemListRow : IModel<int>
    {
        ItemListRowStatus Status { get; }
    }
}
