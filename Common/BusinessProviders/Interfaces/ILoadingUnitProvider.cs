using System;
using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails>
    {
        #region Methods

        IQueryable<LoadingUnitDetails> GetByCellId(int id);

        IQueryable<LoadingUnit> GetWithAreaManual();

        Int32 GetWithAreaManualCount();

        IQueryable<LoadingUnit> GetWithAreaVertimag();

        Int32 GetWithAreaVertimagCount();

        IQueryable<LoadingUnit> GetWithStatusAvailable();

        Int32 GetWithStatusAvailableCount();

        IQueryable<LoadingUnit> GetWithStatusBlocked();

        Int32 GetWithStatusBlockedCount();

        IQueryable<LoadingUnit> GetWithStatusUsed();

        Int32 GetWithStatusUsedCount();

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
