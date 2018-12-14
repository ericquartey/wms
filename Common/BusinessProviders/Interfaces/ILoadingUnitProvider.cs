using System;
using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails>
    {
        #region Methods

        IQueryable<LoadingUnitDetails> GetByCellId(int id);

        IQueryable<ItemList> GetWithAreaManual();

        Int32 GetWithAreaManualCount();

        IQueryable<ItemList> GetWithAreaVertimag();

        Int32 GetWithAreaVertimagCount();

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
