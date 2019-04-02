using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IStatusItemList : IModel<int>
    {
        ItemListStatus Status { get; }
    }
}
