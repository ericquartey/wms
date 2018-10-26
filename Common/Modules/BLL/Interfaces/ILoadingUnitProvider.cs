using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails>
    {
        #region Methods

        bool HasAnyCompartments(int itemId);

        IQueryable<LoadingUnitDetails> GetByCellId(int id);

        #endregion Methods
    }
}
