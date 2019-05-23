using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemPutPolicy : IModel<int>
    {
        #region Properties

        bool HasCompartmentTypes { get; set; }

        #endregion
    }
}
