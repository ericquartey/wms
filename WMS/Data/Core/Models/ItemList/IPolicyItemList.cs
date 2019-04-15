using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IPolicyItemList : IModel<int>
    {
        #region Properties

        ItemListStatus Status { get; }

        #endregion
    }
}
