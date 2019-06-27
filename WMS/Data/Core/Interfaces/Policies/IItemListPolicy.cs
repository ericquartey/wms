using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListPolicy : IModel<int>
    {
        #region Properties

        ItemListStatus Status { get; }

        #endregion
    }
}
