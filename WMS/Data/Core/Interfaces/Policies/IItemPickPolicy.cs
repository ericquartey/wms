using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemPickPolicy : IModel<int>
    {
        #region Properties

        double TotalAvailable { get; }

        #endregion
    }
}
