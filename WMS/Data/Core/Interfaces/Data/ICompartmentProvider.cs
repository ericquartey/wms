using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentProvider :
        ICreateAsyncProvider<CompartmentDetails, int>,
        ICreateRangeAsyncProvider<CompartmentDetails, int>,
        IReadAllPagedAsyncProvider<Compartment, int>,
        IReadSingleAsyncProvider<CompartmentDetails, int>,
        IUpdateAsyncProvider<CompartmentDetails, int>,
        IDeleteAsyncProvider<CompartmentDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IEnumerable<AllowedItemInCompartment>> GetAllowedItemsAsync(int id);

        Task<IEnumerable<Compartment>> GetByItemIdAsync(int id);

        Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id);

        Task<double?> GetMaxCapacityAsync(double width, double depth, int itemId);

        #endregion
    }
}
