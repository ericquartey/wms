using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IPolicyDeleteItemList : IModel<int>
    {
        #region Properties

        bool HasActiveRows { get; }

        ItemListStatus Status { get; }

        #endregion
    }
}
