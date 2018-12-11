using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails>
    {
        #region Methods

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        CompartmentDetails GetNewCompartmentDetails();

        bool HasAnyAllowedItem(int modelId);

        #endregion Methods
    }
}
