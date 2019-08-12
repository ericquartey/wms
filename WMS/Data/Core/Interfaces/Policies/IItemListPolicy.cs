using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListPolicy : IModel<int>
    {
        #region Properties

        Enums.ItemListStatus Status { get; }

        #endregion
    }
}
