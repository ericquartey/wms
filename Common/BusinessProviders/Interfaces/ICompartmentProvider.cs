using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails>
    {
        #region Methods

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        CompartmentDetails GetEnumerationDetails(CompartmentDetails compartmentDetails);

        bool HasAnyAllowedItem(int modelId);

        #endregion Methods
    }
}
