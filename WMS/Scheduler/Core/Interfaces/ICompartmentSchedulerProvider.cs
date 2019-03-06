using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ICompartmentSchedulerProvider
    {
        #region Methods

        Task<StockUpdateCompartment> GetByIdForStockUpdateAsync(int id);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment;

        Task<Compartment> UpdateAsync(Compartment compartment);

        Task<StockUpdateCompartment> UpdateAsync(StockUpdateCompartment compartment);

        #endregion
    }
}
