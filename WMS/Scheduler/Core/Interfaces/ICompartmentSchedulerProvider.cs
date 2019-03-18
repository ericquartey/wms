using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ICompartmentSchedulerProvider
        : IUpdateAsyncProvider<Compartment, int>,
        IUpdateAsyncProvider<StockUpdateCompartment, int>
    {
        #region Methods

        Task<StockUpdateCompartment> GetByIdForStockUpdateAsync(int id);

        IQueryable<Compartment> GetCandidateWithdrawalCompartments(SchedulerRequest schedulerRequest);

        IQueryable<T> OrderCompartmentsByManagementType<T>(IQueryable<T> compartments, ItemManagementType type)
            where T : IOrderableCompartment;

        #endregion
    }
}
