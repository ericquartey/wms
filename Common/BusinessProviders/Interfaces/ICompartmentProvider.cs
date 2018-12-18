using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails>
    {
        #region Methods

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        Task<CompartmentEdit> GetEditableById(int id);

        CompartmentDetails GetNewCompartmentDetails();

        IQueryable<Compartment> GetWithStatusAvailable();

        int GetWithStatusAvailableCount();

        IQueryable<Compartment> GetWithStatusAwaiting();

        int GetWithStatusAwaitingCount();

        IQueryable<Compartment> GetWithStatusBlocked();

        int GetWithStatusBlockedCount();

        IQueryable<Compartment> GetWithStatusExpired();

        int GetWithStatusExpiredCount();

        bool HasAnyAllowedItem(int modelId);

        #endregion Methods
    }
}
