using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListDeletePolicy : IModel<int>
    {
        #region Properties

        bool HasActiveRows { get; }

        Enums.ItemListStatus Status { get; }

        #endregion
    }
}
