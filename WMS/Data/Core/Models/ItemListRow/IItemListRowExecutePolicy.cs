using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IItemListRowExecutePolicy : IModel<int>
    {
        #region Properties

        ItemListRowStatus Status { get; }

        #endregion
    }
}
