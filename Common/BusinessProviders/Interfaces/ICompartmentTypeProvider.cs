using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentTypeProvider : IBusinessProvider<CompartmentType, CompartmentType>
    {
        #region Methods

        Task<OperationResult> AddAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion
    }
}
