using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IAreaDeleteItemArea : IModel<int>
    {
        #region Properties

        double TotalStock { get; }

        #endregion
    }
}
