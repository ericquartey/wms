using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ILoadingUnitWithdrawPolicy : IModel<int>
    {
        #region Properties

        int? CellId { get; }

        #endregion
    }
}
