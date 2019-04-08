using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ICompartmentDeletePolicy : IModel<int>
    {
        #region Properties

        bool IsItemPairingFixed { get; }

        int Stock { get; }

        #endregion
    }
}
