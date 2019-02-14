using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentTypeProvider : IBusinessProvider<CompartmentType, CompartmentType, int>
    {
        #region Methods

        Task<IOperationResult<CompartmentType>> AddAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion
    }
}
