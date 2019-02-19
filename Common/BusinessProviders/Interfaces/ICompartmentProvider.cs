using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider :
        IPagedBusinessProvider<Compartment>,
        IReadSingleAsyncProvider<CompartmentDetails, int>,
        ICreateAsyncProvider<CompartmentDetails>,
        IUpdateAsyncProvider<CompartmentDetails>,
        IDeleteAsyncProvider
    {
        #region Methods

        Task<IOperationResult> AddRangeAsync(IEnumerable<ICompartment> compartments);

        Task<IEnumerable<Compartment>> GetByItemIdAsync(int id);

        Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id);

        Task<int?> GetMaxCapacityAsync(int? width, int? height, int itemId);

        Task<CompartmentDetails> GetNewAsync();

        #endregion
    }
}
