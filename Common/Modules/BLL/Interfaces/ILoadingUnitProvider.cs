using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails>
    {
        #region Methods

        IQueryable<LoadingUnitDetails> GetByCellId(int id);

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
