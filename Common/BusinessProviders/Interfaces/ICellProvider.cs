using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellProvider : IBusinessProvider<Cell, CellDetails>
    {
        #region Methods

        IQueryable<Enumeration> GetByAisleId(int areaId);

        IQueryable<Enumeration> GetByAreaId(int areaId);

        bool HasAnyLoadingUnits(int cellId);

        #endregion Methods
    }
}
