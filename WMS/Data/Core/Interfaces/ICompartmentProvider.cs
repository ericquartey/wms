using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentProvider :
        ICreateAsyncProvider<CompartmentDetails, int>,
        IReadAllPagedAsyncProvider<Compartment, int>,
        IReadSingleAsyncProvider<CompartmentDetails, int>,
        IUpdateAsyncProvider<CompartmentDetails, int>,
        IDeleteAsyncProvider,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<OperationResult<IEnumerable<CompartmentDetails>>> CreateRangeAsync(
            IEnumerable<CompartmentDetails> compartments);

        Task<IEnumerable<AllowedItemInCompartment>> GetAllowedItemsAsync(int id);

        Task<IEnumerable<Compartment>> GetByItemIdAsync(int id);

        Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id);

        Task<int?> GetMaxCapacityAsync(int width, int height, int itemId);

        #endregion
    }
}
