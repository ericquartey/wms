using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemAreaDeletePolicy : IModel<int>
    {
        #region Properties

        bool IsItemInArea { get; }

        #endregion
    }
}
