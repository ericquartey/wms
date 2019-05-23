using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowExecutePolicy : IModel<int>
    {
        #region Properties

        ItemListRowStatus Status { get; }

        #endregion
    }
}
