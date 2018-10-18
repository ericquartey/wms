using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface ILoadingUnitProvider : IBusinessProvider<LoadingUnit, LoadingUnitDetails>
    {
        #region Methods

        bool HasAnyCompartments(int itemId);

        #endregion Methods
    }
}
