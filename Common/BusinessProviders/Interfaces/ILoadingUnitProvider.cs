using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails, int>
    {
        #region Methods

        IQueryable<LoadingUnitDetails> GetByCellId(int id);

        IQueryable<LoadingUnit> GetWithAreaManual();

        int GetWithAreaManualCount();

        IQueryable<LoadingUnit> GetWithAreaVertimag();

        int GetWithAreaVertimagCount();

        IQueryable<LoadingUnit> GetWithStatusAvailable();

        int GetWithStatusAvailableCount();

        IQueryable<LoadingUnit> GetWithStatusBlocked();

        int GetWithStatusBlockedCount();

        IQueryable<LoadingUnit> GetWithStatusUsed();

        int GetWithStatusUsedCount();

        bool HasAnyCompartments(int loadingUnitId);

        #endregion
    }
}
