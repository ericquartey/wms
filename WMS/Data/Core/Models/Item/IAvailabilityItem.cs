using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IAvailabilityItem : IModel<int>
    {
        #region Properties

        int TotalAvailable { get; }

        #endregion
    }
}
