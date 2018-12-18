using System;
using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellProvider : IBusinessProvider<Cell, CellDetails>
    {
        #region Methods

        IQueryable<Enumeration> GetByAisleId(int areaId);

        IQueryable<Enumeration> GetByAreaId(int areaId);

        IQueryable<Cell> GetWithClassA();

        Int32 GetWithClassACount();

        IQueryable<Cell> GetWithStatusEmpty();

        Int32 GetWithStatusEmptyCount();

        IQueryable<Cell> GetWithStatusFull();

        Int32 GetWithStatusFullCount();

        bool HasAnyLoadingUnits(int cellId);

        #endregion Methods
    }
}
