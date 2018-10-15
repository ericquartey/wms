using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails, int>
    {
        #region Methods

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        #endregion Methods
    }
}
