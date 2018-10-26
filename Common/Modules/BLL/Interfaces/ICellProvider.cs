using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface ICellProvider : IBusinessProvider<Cell, CellDetails>
    {
        #region Methods

        bool HasAnyLoadingUnits(int cellId);

        #endregion Methods
    }
}
