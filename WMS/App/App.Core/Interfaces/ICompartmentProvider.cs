using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider :
        IPagedBusinessProvider<Compartment, int>,
        IReadSingleAsyncProvider<CompartmentDetails, int>,
        ICreateAsyncProvider<CompartmentDetails, int>,
        IUpdateAsyncProvider<CompartmentDetails, int>,
        IDeleteAsyncProvider<CompartmentDetails, int>
    {
        #region Methods

        Task<IOperationResult<ICompartment>> AddRangeAsync(IEnumerable<ICompartment> compartments);

        Task<ActionModel> CanDeleteAsync(int id);

        Task<IEnumerable<Compartment>> GetByItemIdAsync(int id);

        Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id);

        Task<int?> GetMaxCapacityAsync(int? width, int? height, int itemId);

        Task<CompartmentDetails> GetNewAsync();

        #endregion
    }
}
