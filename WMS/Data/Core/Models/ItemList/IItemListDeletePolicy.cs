using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IItemListDeletePolicy : IModel<int>
    {
        #region Properties

        bool HasActiveRows { get; }

        ItemListStatus Status { get; }

        #endregion
    }
}
