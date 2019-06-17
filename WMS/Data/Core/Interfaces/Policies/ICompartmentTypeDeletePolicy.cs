using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentTypeDeletePolicy : IModel<int>
    {
        #region Properties

        int CompartmentsCount { get; }

        int ItemCompartmentsCount { get; }

        #endregion
    }
}
