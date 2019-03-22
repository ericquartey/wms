using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
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

        Task<int?> GetMaxCapacityAsync(double? width, double? height, int itemId);

        Task<CompartmentDetails> GetNewAsync();

        #endregion
    }
}
