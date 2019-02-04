using System;
using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellProvider : IBusinessProvider<Cell, CellDetails>
    {
        #region Methods

        IQueryable<Enumeration> GetByAisleId(int aisleId);

        IQueryable<Enumeration> GetByAreaId(int areaId);

        IQueryable<Cell> GetWithClassA();

        int GetWithClassACount();

        IQueryable<Cell> GetWithStatusEmpty();

        int GetWithStatusEmptyCount();

        IQueryable<Cell> GetWithStatusFull();

        int GetWithStatusFullCount();

        bool HasAnyLoadingUnits(int cellId);

        #endregion
    }
}
