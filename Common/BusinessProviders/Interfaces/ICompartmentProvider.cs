using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails, int>
    {
        #region Methods

        Task<IOperationResult<ICompartment>> AddRangeAsync(IEnumerable<ICompartment> compartments);

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        Task<int?> GetMaxCapacityAsync(int? width, int? height, int itemId);

        IQueryable<Compartment> GetWithStatusAvailable();

        int GetWithStatusAvailableCount();

        IQueryable<Compartment> GetWithStatusAwaiting();

        int GetWithStatusAwaitingCount();

        IQueryable<Compartment> GetWithStatusBlocked();

        int GetWithStatusBlockedCount();

        IQueryable<Compartment> GetWithStatusExpired();

        int GetWithStatusExpiredCount();

        bool HasAnyAllowedItem(int modelId);

        #endregion
    }
}
