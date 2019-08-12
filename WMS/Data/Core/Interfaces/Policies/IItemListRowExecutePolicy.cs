using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowExecutePolicy : IModel<int>
    {
        #region Properties

        Enums.ItemListRowStatus Status { get; }

        #endregion
    }
}
