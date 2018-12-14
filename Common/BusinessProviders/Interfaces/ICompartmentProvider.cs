using System;
using System.Linq;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentProvider : IBusinessProvider<Compartment, CompartmentDetails>
    {
        #region Methods

        IQueryable<Compartment> GetByItemId(int id);

        IQueryable<CompartmentDetails> GetByLoadingUnitId(int id);

        CompartmentDetails GetNewCompartmentDetails();

        IQueryable<Compartment> GetWithStatusAvailable();

        Int32 GetWithStatusAvailableCount();

        IQueryable<Compartment> GetWithStatusAwaiting();

        Int32 GetWithStatusAwaitingCount();

        IQueryable<Compartment> GetWithStatusBlocked();

        Int32 GetWithStatusBlockedCount();

        IQueryable<Compartment> GetWithStatusExpired();

        Int32 GetWithStatusExpiredCount();

        bool HasAnyAllowedItem(int modelId);

        #endregion Methods
    }
}
